﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DemoGame.Client.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class ClientSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ClientSettings defaultInstance = ((ClientSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ClientSettings())));
        
        public static ClientSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("127.0.0.1")]
        public string ServerIP {
            get {
                return ((string)(this["ServerIP"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("600000")]
        public uint SyncGameTimeFrequency {
            get {
                return ((uint)(this["SyncGameTimeFrequency"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string EnteredUserName {
            get {
                return ((string)(this["EnteredUserName"]));
            }
            set {
                this["EnteredUserName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string EnteredPassword {
            get {
                return ((string)(this["EnteredPassword"]));
            }
            set {
                this["EnteredPassword"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Left")]
        public global::SFML.Window.KeyCode Keys_MoveLeft {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_MoveLeft"]));
            }
            set {
                this["Keys_MoveLeft"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Right")]
        public global::SFML.Window.KeyCode Keys_MoveRight {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_MoveRight"]));
            }
            set {
                this["Keys_MoveRight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Up")]
        public global::SFML.Window.KeyCode Keys_MoveUp {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_MoveUp"]));
            }
            set {
                this["Keys_MoveUp"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Down")]
        public global::SFML.Window.KeyCode Keys_MoveDown {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_MoveDown"]));
            }
            set {
                this["Keys_MoveDown"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LControl")]
        public global::SFML.Window.KeyCode Keys_Attack {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_Attack"]));
            }
            set {
                this["Keys_Attack"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LAlt")]
        public global::SFML.Window.KeyCode Keys_UseWorld {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_UseWorld"]));
            }
            set {
                this["Keys_UseWorld"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LAlt")]
        public global::SFML.Window.KeyCode Keys_UseShop {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_UseShop"]));
            }
            set {
                this["Keys_UseShop"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LAlt")]
        public global::SFML.Window.KeyCode Keys_TalkToNPC {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_TalkToNPC"]));
            }
            set {
                this["Keys_TalkToNPC"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Space")]
        public global::SFML.Window.KeyCode Keys_PickUp {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_PickUp"]));
            }
            set {
                this["Keys_PickUp"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Num1")]
        public global::SFML.Window.KeyCode Keys_EmoteEllipsis {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_EmoteEllipsis"]));
            }
            set {
                this["Keys_EmoteEllipsis"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Num2")]
        public global::SFML.Window.KeyCode Keys_EmoteExclamation {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_EmoteExclamation"]));
            }
            set {
                this["Keys_EmoteExclamation"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Num3")]
        public global::SFML.Window.KeyCode Keys_EmoteHeartbroken {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_EmoteHeartbroken"]));
            }
            set {
                this["Keys_EmoteHeartbroken"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Num4")]
        public global::SFML.Window.KeyCode Keys_EmoteHearts {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_EmoteHearts"]));
            }
            set {
                this["Keys_EmoteHearts"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Num5")]
        public global::SFML.Window.KeyCode Keys_EmoteMeat {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_EmoteMeat"]));
            }
            set {
                this["Keys_EmoteMeat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Num6")]
        public global::SFML.Window.KeyCode Keys_EmoteQuestion {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_EmoteQuestion"]));
            }
            set {
                this["Keys_EmoteQuestion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Num7")]
        public global::SFML.Window.KeyCode Keys_EmoteSweat {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_EmoteSweat"]));
            }
            set {
                this["Keys_EmoteSweat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F1")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem0 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem0"]));
            }
            set {
                this["Keys_QuickBarItem0"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F2")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem1 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem1"]));
            }
            set {
                this["Keys_QuickBarItem1"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F3")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem2 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem2"]));
            }
            set {
                this["Keys_QuickBarItem2"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F4")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem3 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem3"]));
            }
            set {
                this["Keys_QuickBarItem3"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F5")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem4 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem4"]));
            }
            set {
                this["Keys_QuickBarItem4"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F6")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem5 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem5"]));
            }
            set {
                this["Keys_QuickBarItem5"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F7")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem6 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem6"]));
            }
            set {
                this["Keys_QuickBarItem6"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F8")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem7 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem7"]));
            }
            set {
                this["Keys_QuickBarItem7"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F9")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem8 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem8"]));
            }
            set {
                this["Keys_QuickBarItem8"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F10")]
        public global::SFML.Window.KeyCode Keys_QuickBarItem9 {
            get {
                return ((global::SFML.Window.KeyCode)(this["Keys_QuickBarItem9"]));
            }
            set {
                this["Keys_QuickBarItem9"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("60")]
        public byte Audio_SoundVolume {
            get {
                return ((byte)(this["Audio_SoundVolume"]));
            }
            set {
                this["Audio_SoundVolume"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public byte Audio_MusicVolume {
            get {
                return ((byte)(this["Audio_MusicVolume"]));
            }
            set {
                this["Audio_MusicVolume"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool Graphics_VSync {
            get {
                return ((bool)(this["Graphics_VSync"]));
            }
            set {
                this["Graphics_VSync"] = value;
            }
        }
    }
}
