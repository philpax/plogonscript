using System;
using Dalamud.Utility.Signatures;

namespace PlogonScript.Script.Bindings;

internal unsafe class SoundImplementation
{
    // 0x1400A55B0: Client::UI::PlaySoundEffect # this is a static function in the UI namespace, arg1 is the SE
    [Signature("E8 ?? ?? ?? ?? 4D 39 BE ?? ?? ?? ??")]
    public readonly delegate* unmanaged<uint, IntPtr, IntPtr, byte, void> PlaySoundEffect = null;

    internal SoundImplementation()
    {
        SignatureHelper.Initialise(this);
    }
}

public class Sound
{
    private static readonly SoundImplementation _impl = new();

    public static void PlayEffect(uint soundEffectId)
    {
        unsafe
        {
            _impl.PlaySoundEffect(soundEffectId, IntPtr.Zero, IntPtr.Zero, 0);
        }
    }
}