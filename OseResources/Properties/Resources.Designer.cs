//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4952
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OseResources.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OseResources.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OneStory Editor Projects.
        /// </summary>
        public static string DefMyDocsSubfolder {
            get {
                return ResourceManager.GetString("DefMyDocsSubfolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;button id=&quot;{0}&quot; onClick=&quot;{1}&quot;&gt;{2}&lt;/button&gt;.
        /// </summary>
        public static string HTML_Button {
            get {
                return ResourceManager.GetString("HTML_Button", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;button id=&quot;{0}&quot; style=&quot;position:relative; left:20px; height:20px; width:20px;&quot; oncontextmenu=&quot;return showContextMenu(this);&quot;&gt;{1}&lt;/button&gt;.
        /// </summary>
        public static string HTML_ButtonRightAlignCtxMenu {
            get {
                return ResourceManager.GetString("HTML_ButtonRightAlignCtxMenu", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;script&gt;
        ///    function OnBibRefJump(link) {
        ///        window.external.OnBibRefJump(link.name);
        ///        return false; // cause the href navigation to not happen
        ///    }
        ///    function OnVerseLineJump(link) {
        ///        window.external.OnVerseLineJump(link.name);
        ///        return false; // cause the href navigation to not happen
        ///    }
        ///    function OnKeyDown() {
        ///        if (window.event.keyCode == 116) {
        ///            // let the form handle it
        ///            window.external.LoadDocument();
        ///
        ///            // disable [rest of string was truncated]&quot;;.
        /// </summary>
        public static string HTML_DOM_Prefix {
            get {
                return ResourceManager.GetString("HTML_DOM_Prefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;script type=&quot;text/javascript&quot;&gt;
        ///function OnBibRefJump(btn)
        ///{
        ///  window.external.OnBibRefJump(btn.id);
        ///  return false; // cause the href navigation to not happen
        ///}
        ///function OnVerseLineJump(link)
        ///{
        ///  window.external.OnVerseLineJump(link.name);
        ///  return false; // cause the href navigation to not happen
        ///}
        ///function OnKeyDown()
        ///{
        ///  if (window.event.keyCode == 116)
        ///  {
        ///    // let the form handle it
        ///    window.external.LoadDocument();
        ///
        ///    // disable the propagation of the F5 event
        ///    window.even [rest of string was truncated]&quot;;.
        /// </summary>
        public static string HTML_DOM_PrefixPresentation {
            get {
                return ResourceManager.GetString("HTML_DOM_PrefixPresentation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;script type=&quot;text/javascript&quot;&gt;
        ///
        ////************************************************************************************************************
        ///(C) www.dhtmlgoodies.com, October 2005
        ///
        ///This is a script from www.dhtmlgoodies.com. You will find this and a lot of other scripts at our website.	
        ///
        ///Terms of use:
        ///You are free to use this script as long as the copyright message is kept intact. However, you may not
        ///redistribute, sell or repost it without our permission.
        ///
        ///Thank you!
        ///
        ///www.dhtmlgoodies.com
        ///A [rest of string was truncated]&quot;;.
        /// </summary>
        public static string HTML_DOM_PrefixStoryBt {
            get {
                return ResourceManager.GetString("HTML_DOM_PrefixStoryBt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;em&gt;{0}&lt;/em&gt;.
        /// </summary>
        public static string HTML_EmphasizedText {
            get {
                return ResourceManager.GetString("HTML_EmphasizedText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;html&gt;
        ///{0}
        ///&lt;head&gt;
        ///{1}
        ///&lt;/head&gt;
        ///&lt;body onKeyDown=&quot;return OnKeyDown();&quot; onscroll=&quot;window.external.OnScroll()&quot;&gt;
        ///{2}
        ///&lt;/body&gt;
        ///&lt;/html&gt;.
        /// </summary>
        public static string HTML_Header {
            get {
                return ResourceManager.GetString("HTML_Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;html&gt;
        ///{0}
        ///&lt;head&gt;
        ///{1}
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///{2}
        ///&lt;/body&gt;
        ///&lt;/html&gt;.
        /// </summary>
        public static string HTML_HeaderPresentation {
            get {
                return ResourceManager.GetString("HTML_HeaderPresentation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;html&gt;
        ///{0}
        ///&lt;head&gt;
        ///{1}
        ///&lt;/head&gt;
        ///&lt;body onKeyDown=&quot;return OnKeyDown();&quot; onscroll=&quot;window.external.OnScroll();&quot;&gt;
        ///&lt;ul id=&quot;contextMenu&quot;&gt;
        ///	&lt;li&gt;&lt;a href=&quot;#&quot; onclick=&quot;window.external.InsertNewVerseBefore(nVerseIndex);&quot;&gt;Insert new verse before this one&lt;/a&gt;&lt;/li&gt;
        ///	&lt;li&gt;&lt;a href=&quot;#&quot; onclick=&quot;window.external.AddNewVerseAfter(nVerseIndex);&quot;&gt;Add new verse after this one&lt;/a&gt;&lt;/li&gt;
        ///	&lt;li&gt;&lt;a href=&quot;#&quot; onclick=&quot;window.external.HideVerse(nVerseIndex);&quot;&gt;Hide verse&lt;/a&gt;&lt;/li&gt;
        ///	&lt;li&gt;&lt;a href=&quot;#&quot; onclick=&quot;window.external.DeleteVerse [rest of string was truncated]&quot;;.
        /// </summary>
        public static string HTML_HeaderStoryBt {
            get {
                return ResourceManager.GetString("HTML_HeaderStoryBt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;a href=&quot;{0}&quot; target=&quot;_blank&quot;&gt;here&lt;/a&gt; .
        /// </summary>
        public static string HTML_HttpLink {
            get {
                return ResourceManager.GetString("HTML_HttpLink", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .{0} {{
        ///      font-family: &quot;{1}&quot;;
        ///      font-size: {2} pt;
        ///      color: {3};
        ///      direction: {4};
        ///      text-align: {5};
        ///  }}.
        /// </summary>
        public static string HTML_LangStyle {
            get {
                return ResourceManager.GetString("HTML_LangStyle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;a href=&quot;conNote.jumpToLine&quot; name=&quot;{0}&quot; onClick=&quot;return OnVerseLineJump(this);&quot;&gt;{1}&lt;/a&gt;.
        /// </summary>
        public static string HTML_LinkJumpLine {
            get {
                return ResourceManager.GetString("HTML_LinkJumpLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;a href=&quot;bibleViewer.setReference&quot; name=&quot;{0}&quot; onClick=&quot;return OnBibRefJump(this);&quot;&gt;{0}&lt;/a&gt;.
        /// </summary>
        public static string HTML_LinkJumpTargetBibleReference {
            get {
                return ResourceManager.GetString("HTML_LinkJumpTargetBibleReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;p id=&quot;{0}&quot; class=&quot;{1}&quot;&gt;{2}&lt;/p&gt;.
        /// </summary>
        public static string HTML_ParagraphText {
            get {
                return ResourceManager.GetString("HTML_ParagraphText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;style TYPE=&quot;text/css&quot;&gt;
        ///  textarea {{ 
        ///      width:100%; 
        ///      overflow:visible
        ///  }}
        ///{0}
        ///  #contextMenu {{	/* The menu container */
        ///    border:1px solid #202867;	/* Border around the entire menu */
        ///    background-color:#FFF;	/* White background color of the menu */
        ///    margin:0px;
        ///    padding:0px;
        ///    width:300px;	/* Width of context menu */
        ///    font-family:arial;
        ///    font-size:11px;
        ///    background-image:url(&apos;gradient.gif&apos;);
        ///    background-repeat:repeat-y;
        ///    	
        ///    /* Never change these t [rest of string was truncated]&quot;;.
        /// </summary>
        public static string HTML_StyleDefinition {
            get {
                return ResourceManager.GetString("HTML_StyleDefinition", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to   &lt;table style=&quot;width:100%;&quot; border=&quot;1&quot;&gt;
        ///  {0}
        ///  &lt;/table&gt;.
        /// </summary>
        public static string HTML_Table {
            get {
                return ResourceManager.GetString("HTML_Table", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td&gt;{0}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCell {
            get {
                return ResourceManager.GetString("HTML_TableCell", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td width=&quot;100%&quot; BGCOLOR={0}&gt;{1}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCellForTextArea {
            get {
                return ResourceManager.GetString("HTML_TableCellForTextArea", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td NOWRAP&gt;{0}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCellNoWrap {
            get {
                return ResourceManager.GetString("HTML_TableCellNoWrap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td width=&quot;{0}%&quot;&gt;{1}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCellWidth {
            get {
                return ResourceManager.GetString("HTML_TableCellWidth", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td width=&quot;{0}%&quot; valign=&quot;top&quot;&gt;{1}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCellWidthAlignTop {
            get {
                return ResourceManager.GetString("HTML_TableCellWidthAlignTop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td id=&quot;{0}&quot; width=&quot;{1}%&quot;&gt;{2}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCellWidthId {
            get {
                return ResourceManager.GetString("HTML_TableCellWidthId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td id=&quot;{0}&quot; width=&quot;{1}%&quot; colspan=&quot;{2}&quot;&gt;{3}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCellWidthSpanId {
            get {
                return ResourceManager.GetString("HTML_TableCellWidthSpanId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td colspan=&quot;{0}&quot;&gt;{1}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCellWithSpan {
            get {
                return ResourceManager.GetString("HTML_TableCellWithSpan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;td width=&quot;{0}%&quot; colspan=&quot;{1}&quot;&gt;{2}&lt;/td&gt;.
        /// </summary>
        public static string HTML_TableCellWithSpanAndWidth {
            get {
                return ResourceManager.GetString("HTML_TableCellWithSpanAndWidth", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to         &lt;table style=&quot;width:100%;&quot;&gt;
        ///        {0}
        ///        &lt;/table&gt;
        ///.
        /// </summary>
        public static string HTML_TableNoBorder {
            get {
                return ResourceManager.GetString("HTML_TableNoBorder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to   &lt;tr&gt;
        ///    {0}
        ///  &lt;/tr&gt;
        ///    .
        /// </summary>
        public static string HTML_TableRow {
            get {
                return ResourceManager.GetString("HTML_TableRow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;tr BGCOLOR=&quot;{0}&quot;&gt;{1}&lt;/tr&gt;.
        /// </summary>
        public static string HTML_TableRowColor {
            get {
                return ResourceManager.GetString("HTML_TableRowColor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;tr id=&quot;{0}&quot;&gt;{1}&lt;/tr&gt;    .
        /// </summary>
        public static string HTML_TableRowId {
            get {
                return ResourceManager.GetString("HTML_TableRowId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;tr id=&quot;{0}&quot; BGCOLOR=&quot;{1}&quot;&gt;{2}&lt;/tr&gt;    .
        /// </summary>
        public static string HTML_TableRowIdColor {
            get {
                return ResourceManager.GetString("HTML_TableRowIdColor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;textarea id=&quot;{0}&quot; class=&quot;{1}&quot; onKeyUp=&quot;return window.external.TextareaOnKeyUp(this.id, this.value);&quot;&gt;{2}&lt;/textarea&gt;.
        /// </summary>
        public static string HTML_Textarea {
            get {
                return ResourceManager.GetString("HTML_Textarea", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;textarea id=&quot;{0}&quot; class=&quot;{1}&quot; onKeyUp=&quot;return window.external.TextareaOnKeyUp(this.id, this.value);&quot; ondragover=&quot;window.event.returnValue=false&quot; ondrop=&quot;window.external.CopyScriptureReference(this.id)&quot; onselect=&quot;this.focus();&quot;&gt;{2}&lt;/textarea&gt;.
        /// </summary>
        public static string HTML_TextareaWithRefDrop {
            get {
                return ResourceManager.GetString("HTML_TextareaWithRefDrop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Follow these steps to use the Adapt It program to translate the story into English:{0}{0}1) Switch to the Adapt It program using Alt+Tab keys{0}2) Select the &apos;{1}&apos; project and click the &apos;Next&apos; button (if a file is already open in Adapt It, then choose the &apos;File&apos; menu, &apos;Close Project&apos; command and then the &apos;File&apos; menu, &apos;Start Working&apos; command first).{0}3) Select the &apos;{2}.xls&apos; document to open and press the &apos;Finished&apos; button.{0}4) When you see this story in the adaptation window, then translate it into English [rest of string was truncated]&quot;;.
        /// </summary>
        public static string IDS_AdaptationInstructions {
            get {
                return ResourceManager.GetString("IDS_AdaptationInstructions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OneStory Project Editor.
        /// </summary>
        public static string IDS_Caption {
            get {
                return ResourceManager.GetString("IDS_Caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {\rtf1\adeflang1081\ansi\ansicpg1252\uc1\adeff0\deff0\stshfdbch0\stshfloch0\stshfhich0\stshfbi0\deflang1033\deflangfe1033{\fonttbl{\f0\froman\fcharset0\fprq2{\*\panose 02020603050405020304}Times New Roman;}{\f312\froman\fcharset238\fprq2 Times New Roman CE;}{\f313\froman\fcharset204\fprq2 Times New Roman Cyr;}
        ///{\f315\froman\fcharset161\fprq2 Times New Roman Greek;}{\f316\froman\fcharset162\fprq2 Times New Roman Tur;}{\f317\fbidi \froman\fcharset177\fprq2 Times New Roman (Hebrew);}{\f318\fbidi \froman\fchar [rest of string was truncated]&quot;;.
        /// </summary>
        public static string IDS_DefaultPanoramaFrontMatter {
            get {
                return ResourceManager.GetString("IDS_DefaultPanoramaFrontMatter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to One of the team members has used a newer version of this program to edit the file, which is not compatible with the version you are using. You will have to go to the http://palaso.org/install/onestory website and download and install the new version of the program in the &quot;Setup OneStory Editor.zip&quot; file..
        /// </summary>
        public static string IDS_GetNewVersion {
            get {
                return ResourceManager.GetString("IDS_GetNewVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have to log in to continue. Click &apos;Project&apos;, &apos;Login&apos; and choose or add your name..
        /// </summary>
        public static string IDS_HaveToLogInToContinue {
            get {
                return ResourceManager.GetString("IDS_HaveToLogInToContinue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  (Hidden).
        /// </summary>
        public static string IDS_HiddenLabel {
            get {
                return ResourceManager.GetString("IDS_HiddenLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The project file has an ill-formed Anchor! Wasn&apos;t expecting to see &apos;{0}&apos;. Contact bob_eaton@sall.com and mention which the project name..
        /// </summary>
        public static string IDS_IllFormedJumpTarget {
            get {
                return ResourceManager.GetString("IDS_IllFormedJumpTarget", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stories.
        /// </summary>
        public static string IDS_MainStoriesSet {
            get {
                return ResourceManager.GetString("IDS_MainStoriesSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  was visible, but now is hidden.
        /// </summary>
        public static string IDS_NowIsHidden {
            get {
                return ResourceManager.GetString("IDS_NowIsHidden", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Old Stories.
        /// </summary>
        public static string IDS_ObsoleteStoriesSet {
            get {
                return ResourceManager.GetString("IDS_ObsoleteStoriesSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The project file is corrupted. No &apos;StoryCrafterMemberID&apos; record found. Send to bob_eaton@sall.com for help..
        /// </summary>
        public static string IDS_ProjectFileCorrupted {
            get {
                return ResourceManager.GetString("IDS_ProjectFileCorrupted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The project file is corrupted. No &apos;CraftingInfo&apos; record found. Send to bob_eaton@sall.com for help..
        /// </summary>
        public static string IDS_ProjectFileCorruptedNoCraftingInfo {
            get {
                return ResourceManager.GetString("IDS_ProjectFileCorruptedNoCraftingInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  was hidden, but now is visible.
        /// </summary>
        public static string IDS_WasHidden {
            get {
                return ResourceManager.GetString("IDS_WasHidden", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Right now, only a &apos;{0}&apos; can change the state of this story. If you&apos;re a {0}, click &apos;Project&apos;, &apos;Login&apos; to login. You can log in as a &apos;Just Looking&apos; member to be able to transition to any state, but without edit privileges..
        /// </summary>
        public static string IDS_WhichUserEdits {
            get {
                return ResourceManager.GetString("IDS_WhichUserEdits", resourceCulture);
            }
        }
    }
}
