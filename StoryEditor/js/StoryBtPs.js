var textareas = document.getElementsByTagName("textarea");
for (var i=0; i < textareas.length; i++)
{
    textareas[i].onchange = function() { return window.external.TextareaOnChange(this.id, this.value); };
    textareas[i].onselect = function () { return CallTextareaOnSelect(this); };
}
$('textarea').attr('placeholder', function () {
    if ($(this).hasClass('LangVernacular'))
        return VernacularLanguageName();
    else if ($(this).hasClass('LangNationalBt'))
        return NationalBtLanguageName();
    else if ($(this).hasClass('LangInternationalBt'))
        return InternationalBtLanguageName();
    else if ($(this).hasClass('LangFreeTranslation'))
        return FreeTranslationLanguageName();
    else
        return "error in StoryBtPs.js";
});
