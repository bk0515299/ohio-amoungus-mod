using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

[BepInPlugin("com.winston.ohioeverything", "Ohio Everything Mod", "1.0.0")]
public class OhioEverythingMod : BasePlugin
{
    private static AudioClip ohioClip;

    public override void Load()
    {
        Harmony harmony = new Harmony("com.winston.ohioeverything");
        harmony.PatchAll();

        Log.LogInfo("Ohio Everything Mod loaded successfully");
    }

    // =========================================================
    // 🔤 REPLACE ALL TEXT WITH OHIO
    // =========================================================
    [HarmonyPatch(typeof(TMP_Text))]
    class ReplaceAllText
    {
        [HarmonyPatch("set_text")]
        [HarmonyPrefix]
        static void Prefix(ref string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            value = Ohioify(value);
        }

        static string Ohioify(string input)
        {
            return input
                .Replace("Among Us", "OHIO")
                .Replace("AMONG US", "OHIO")
                .Replace("Crewmate", "Ohio Mate")
                .Replace("CREWMATE", "OHIO MATE")
                .Replace("Impostor", "Ohio")
                .Replace("IMPOSTOR", "OHIO")
                .Replace("Settings", "Ohio Settings");
        }
    }

    // =========================================================
    // 🎵 REPLACE MENU MUSIC (OVERLOAD-SAFE)
    // =========================================================
    [HarmonyPatch(typeof(AudioSource))]
    class ReplaceMusic
    {
        [HarmonyPatch(nameof(AudioSource.Play), new Type[] { })]
        [HarmonyPrefix]
        static void Prefix(AudioSource __instance)
        {
            if (__instance == null) return;
            if (!__instance.loop) return;

            if (ohioClip == null)
            {
                ohioClip = LoadWavFromResource(
                    "OhioEverythingMod.Audio.ohio.wav"
                );
            }

            if (ohioClip != null)
            {
                __instance.clip = ohioClip;
            }
        }
    }

    // =========================================================
    // 🅾️ REPLACE LOGO WITH TEXT
    // =========================================================
    [HarmonyPatch(typeof(Image))]
    class ReplaceLogo
    {
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        static void Postfix(Image __instance)
        {
            if (__instance == null) return;
            if (!__instance.name.ToLower().Contains("logo")) return;

            __instance.enabled = false;

            GameObject go = new GameObject("OhioLogo");
            go.transform.SetParent(__instance.transform.parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = "OHIO";
            text.fontSize = 120;
            text.color = Color.red;
            text.alignment = TextAlignmentOptions.Center;
        }
    }

    // =========================================================
    // 🎧 EMBEDDED WAV LOADER
    // =========================================================
    static AudioClip LoadWavFromResource(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        byte[] wav = new byte[stream.Length];
        stream.Read(wav, 0, wav.Length);

        int channels = wav[22];
        int sampleRate = BitConverter.ToInt32(wav, 24);
        int dataOffset = 44;

        int sampleCount = (wav.Length - dataOffset) / 2;
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(wav, dataOffset + i * 2);
            samples[i] = sample / 32768f;
        }

        var clip = AudioClip.Create(
            "OhioMusic",
            sampleCount / channels,
            channels,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);
        return clip;
    }
}
