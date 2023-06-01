//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/000 - IndigenousGames/006 - Scripts/000 - GameManager/GameController.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @GameController : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @GameController()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""GameController"",
    ""maps"": [
        {
            ""name"": ""AgawanBase"",
            ""id"": ""95e89618-0588-4dda-a3b5-cb97f8da5dd2"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""PassThrough"",
                    ""id"": ""813160c0-b97f-4a73-b83c-7614082e8f82"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CameraLook"",
                    ""type"": ""PassThrough"",
                    ""id"": ""20529096-3139-4a8f-9bba-a67c865b8d0c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""d9b808c3-1304-4298-a0f0-e62bef414422"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Android"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""85841f40-661b-40f7-8f26-ce07e2a750d5"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Android"",
                    ""action"": ""CameraLook"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Android"",
            ""bindingGroup"": ""Android"",
            ""devices"": [
                {
                    ""devicePath"": ""<AndroidGamepad>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // AgawanBase
        m_AgawanBase = asset.FindActionMap("AgawanBase", throwIfNotFound: true);
        m_AgawanBase_Movement = m_AgawanBase.FindAction("Movement", throwIfNotFound: true);
        m_AgawanBase_CameraLook = m_AgawanBase.FindAction("CameraLook", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // AgawanBase
    private readonly InputActionMap m_AgawanBase;
    private IAgawanBaseActions m_AgawanBaseActionsCallbackInterface;
    private readonly InputAction m_AgawanBase_Movement;
    private readonly InputAction m_AgawanBase_CameraLook;
    public struct AgawanBaseActions
    {
        private @GameController m_Wrapper;
        public AgawanBaseActions(@GameController wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_AgawanBase_Movement;
        public InputAction @CameraLook => m_Wrapper.m_AgawanBase_CameraLook;
        public InputActionMap Get() { return m_Wrapper.m_AgawanBase; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(AgawanBaseActions set) { return set.Get(); }
        public void SetCallbacks(IAgawanBaseActions instance)
        {
            if (m_Wrapper.m_AgawanBaseActionsCallbackInterface != null)
            {
                @Movement.started -= m_Wrapper.m_AgawanBaseActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_AgawanBaseActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_AgawanBaseActionsCallbackInterface.OnMovement;
                @CameraLook.started -= m_Wrapper.m_AgawanBaseActionsCallbackInterface.OnCameraLook;
                @CameraLook.performed -= m_Wrapper.m_AgawanBaseActionsCallbackInterface.OnCameraLook;
                @CameraLook.canceled -= m_Wrapper.m_AgawanBaseActionsCallbackInterface.OnCameraLook;
            }
            m_Wrapper.m_AgawanBaseActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
                @CameraLook.started += instance.OnCameraLook;
                @CameraLook.performed += instance.OnCameraLook;
                @CameraLook.canceled += instance.OnCameraLook;
            }
        }
    }
    public AgawanBaseActions @AgawanBase => new AgawanBaseActions(this);
    private int m_AndroidSchemeIndex = -1;
    public InputControlScheme AndroidScheme
    {
        get
        {
            if (m_AndroidSchemeIndex == -1) m_AndroidSchemeIndex = asset.FindControlSchemeIndex("Android");
            return asset.controlSchemes[m_AndroidSchemeIndex];
        }
    }
    public interface IAgawanBaseActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnCameraLook(InputAction.CallbackContext context);
    }
}