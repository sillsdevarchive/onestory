//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3082
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OneStoryProjectEditor.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string LastUserType {
            get {
                return ((string)(this["LastUserType"]));
            }
            set {
                this["LastUserType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastNetBibleReference {
            get {
                return ((string)(this["LastNetBibleReference"]));
            }
            set {
                this["LastNetBibleReference"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastStoryWorkedOn {
            get {
                return ((string)(this["LastStoryWorkedOn"]));
            }
            set {
                this["LastStoryWorkedOn"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastMemberLogin {
            get {
                return ((string)(this["LastMemberLogin"]));
            }
            set {
                this["LastMemberLogin"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection RecentFiles {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["RecentFiles"]));
            }
            set {
                this["RecentFiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastProjectFile {
            get {
                return ((string)(this["LastProjectFile"]));
            }
            set {
                this["LastProjectFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"By this stage, you will have already crafted and recorded a story according to the procedure 
you learned in the workshop. Now you (the crafter) must find an uninitiated native speaker (UNS) 
to listen to the story line-by-line back-translate it into the national language. Then you will type 
that back-translation into the verse field(s) in this window.

You can type the entire story (back-translation) into a single verse box. Then when you're 
finished, right click on the small button in the upper right corner of the verse box (known
as the 'verse commands' button) and choose 'Split story into verses'. A question box will 
be displays which asks for the character in that language used to end a sentence (e.g. 
a 'period' in English, or a danda in Hindi). This command will automatically split the story 
into verses.

You can also manually add new verse boxes by right-clicking on that same 'verse 
commands' button and choose either 'Insert new verse(s) before' or 'Add new verse(s) after'.
Each of these menus has a sub-menu that allows you to insert more than one at a time.

When you are finished entering or splitting the story into separate verses of about one 
sentence per verse, then click the button in the left-hand side of the status bar and choose 
'Next Stage' to go to the next stage in the process.")]
        public string HelpTooltipCrafterTypeNationalBT {
            get {
                return ((string)(this["HelpTooltipCrafterTypeNationalBT"]));
            }
            set {
                this["HelpTooltipCrafterTypeNationalBT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Crafter enters National language back translation")]
        public string HelpCrafterTypeNationalBT {
            get {
                return ((string)(this["HelpCrafterTypeNationalBT"]));
            }
            set {
                this["HelpCrafterTypeNationalBT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Jubilee Hindi Bible, Fish Bible, New Living Translation, Genesis Film")]
        public string LastResourcesUsed {
            get {
                return ((string)(this["LastResourcesUsed"]));
            }
            set {
                this["LastResourcesUsed"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Crafter enters English language back translation")]
        public string HelpCrafterTypeInternationalBT {
            get {
                return ((string)(this["HelpCrafterTypeInternationalBT"]));
            }
            set {
                this["HelpCrafterTypeInternationalBT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"In this stage, an uninitiated native speaker (UNS) will back translate
the story into national language and one of the story crafters will type 
that back translation into the verse field(s)
 in this window. You can 
right-click on the small button in the upper right corner of the verse
pane and choose 'Add new verse(s) after', '<num>' or so to insert 
some empty verse fields for the story. Alternate, you can type the 
entire story into a single verse box and then right click on the same
small 'verse commands' button and choose '
'.
This will automatically split the story into verses when you tell it
which character to use as an 'end-of-sentence' character.

When you are finished entering or splitting the story into separate 
verses of about one sentence per verse, then click the button in 
the left-hand side of the status bar and choose 'Next Stage' to go 
to the next stage in the process.")]
        public string HelpTooltipCrafterTypeInternationalBT {
            get {
                return ((string)(this["HelpTooltipCrafterTypeInternationalBT"]));
            }
            set {
                this["HelpTooltipCrafterTypeInternationalBT"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ftp://ftp.seedconnect.org/HgRep/")]
        public string RemoteUrl {
            get {
                return ((string)(this["RemoteUrl"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Bob_Eaton")]
        public string RemoteUrlUsername {
            get {
                return ((string)(this["RemoteUrlUsername"]));
            }
            set {
                this["RemoteUrlUsername"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("tsc2009")]
        public string RemoteUrlPassword {
            get {
                return ((string)(this["RemoteUrlPassword"]));
            }
            set {
                this["RemoteUrlPassword"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("OneStory")]
        public string DefMyDocsSubfolder {
            get {
                return ((string)(this["DefMyDocsSubfolder"]));
            }
        }
    }
}
