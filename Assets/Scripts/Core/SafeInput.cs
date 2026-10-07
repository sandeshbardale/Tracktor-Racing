using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TractorRacing
{
    public static class SafeInput
    {
        public static bool GetKey(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                switch (key)
                {
                    case KeyCode.W: return Keyboard.current.wKey.isPressed;
                    case KeyCode.S: return Keyboard.current.sKey.isPressed;
                    case KeyCode.A: return Keyboard.current.aKey.isPressed;
                    case KeyCode.D: return Keyboard.current.dKey.isPressed;
                    case KeyCode.UpArrow: return Keyboard.current.upArrowKey.isPressed;
                    case KeyCode.DownArrow: return Keyboard.current.downArrowKey.isPressed;
                    case KeyCode.LeftArrow: return Keyboard.current.leftArrowKey.isPressed;
                    case KeyCode.RightArrow: return Keyboard.current.rightArrowKey.isPressed;
                    case KeyCode.Space: return Keyboard.current.spaceKey.isPressed;
                    case KeyCode.H: return Keyboard.current.hKey.isPressed;
                    case KeyCode.R: return Keyboard.current.rKey.isPressed;
                    case KeyCode.V: return Keyboard.current.vKey.isPressed;
                    case KeyCode.B: return Keyboard.current.bKey.isPressed;
                    case KeyCode.C: return Keyboard.current.cKey.isPressed;
                    case KeyCode.L: return Keyboard.current.lKey.isPressed;
                    case KeyCode.T: return Keyboard.current.tKey.isPressed;
                    case KeyCode.Escape: return Keyboard.current.escapeKey.isPressed;
                    case KeyCode.Return: case KeyCode.KeypadEnter: return Keyboard.current.enterKey.isPressed || Keyboard.current.numpadEnterKey.isPressed;
                    case KeyCode.Alpha1: case KeyCode.Keypad1: return Keyboard.current.digit1Key.isPressed || Keyboard.current.numpad1Key.isPressed;
                    case KeyCode.Alpha2: case KeyCode.Keypad2: return Keyboard.current.digit2Key.isPressed || Keyboard.current.numpad2Key.isPressed;
                    case KeyCode.Alpha3: case KeyCode.Keypad3: return Keyboard.current.digit3Key.isPressed || Keyboard.current.numpad3Key.isPressed;
                    case KeyCode.Alpha4: case KeyCode.Keypad4: return Keyboard.current.digit4Key.isPressed || Keyboard.current.numpad4Key.isPressed;
                    case KeyCode.Alpha5: case KeyCode.Keypad5: return Keyboard.current.digit5Key.isPressed || Keyboard.current.numpad5Key.isPressed;
                }
            }
#endif
            try
            {
                return Input.GetKey(key);
            }
            catch
            {
                return false;
            }
        }

        public static bool GetKeyDown(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                switch (key)
                {
                    case KeyCode.W: return Keyboard.current.wKey.wasPressedThisFrame;
                    case KeyCode.S: return Keyboard.current.sKey.wasPressedThisFrame;
                    case KeyCode.A: return Keyboard.current.aKey.wasPressedThisFrame;
                    case KeyCode.D: return Keyboard.current.dKey.wasPressedThisFrame;
                    case KeyCode.UpArrow: return Keyboard.current.upArrowKey.wasPressedThisFrame;
                    case KeyCode.DownArrow: return Keyboard.current.downArrowKey.wasPressedThisFrame;
                    case KeyCode.LeftArrow: return Keyboard.current.leftArrowKey.wasPressedThisFrame;
                    case KeyCode.RightArrow: return Keyboard.current.rightArrowKey.wasPressedThisFrame;
                    case KeyCode.Space: return Keyboard.current.spaceKey.wasPressedThisFrame;
                    case KeyCode.H: return Keyboard.current.hKey.wasPressedThisFrame;
                    case KeyCode.R: return Keyboard.current.rKey.wasPressedThisFrame;
                    case KeyCode.V: return Keyboard.current.vKey.wasPressedThisFrame;
                    case KeyCode.B: return Keyboard.current.bKey.wasPressedThisFrame;
                    case KeyCode.C: return Keyboard.current.cKey.wasPressedThisFrame;
                    case KeyCode.L: return Keyboard.current.lKey.wasPressedThisFrame;
                    case KeyCode.T: return Keyboard.current.tKey.wasPressedThisFrame;
                    case KeyCode.Escape: return Keyboard.current.escapeKey.wasPressedThisFrame;
                    case KeyCode.Return: case KeyCode.KeypadEnter: return Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;
                    case KeyCode.Alpha1: case KeyCode.Keypad1: return Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame;
                    case KeyCode.Alpha2: case KeyCode.Keypad2: return Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame;
                    case KeyCode.Alpha3: case KeyCode.Keypad3: return Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame;
                    case KeyCode.Alpha4: case KeyCode.Keypad4: return Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame;
                    case KeyCode.Alpha5: case KeyCode.Keypad5: return Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame;
                }
            }
#endif
            try
            {
                return Input.GetKeyDown(key);
            }
            catch
            {
                return false;
            }
        }

        public static float GetVerticalAxis()
        {
            float val = 0f;
            if (GetKey(KeyCode.W) || GetKey(KeyCode.UpArrow)) val += 1f;
            if (GetKey(KeyCode.S) || GetKey(KeyCode.DownArrow)) val -= 1f;

            if (Mathf.Abs(val) < 0.01f)
            {
                try { val = Input.GetAxis("Vertical"); } catch { }
            }
            return val;
        }

        public static float GetHorizontalAxis()
        {
            float val = 0f;
            if (GetKey(KeyCode.D) || GetKey(KeyCode.RightArrow)) val += 1f;
            if (GetKey(KeyCode.A) || GetKey(KeyCode.LeftArrow)) val -= 1f;

            if (Mathf.Abs(val) < 0.01f)
            {
                try { val = Input.GetAxis("Horizontal"); } catch { }
            }
            return val;
        }
    }
}
