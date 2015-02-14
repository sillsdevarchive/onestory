// this one is called from the anchor buttons
function OnBibRefJump(btn) {
    if (event.button == 2) {
        window.external.OnAnchorButton(btn.id);

        // prevent the OnEmptyAnchorClick from happening too
        event.cancelBubble = true;
    }
    else
        window.external.OnBibRefJump(btn.name);
    return false; // cause the href navigation to not happen
}
// this one is called from the empty cell where the buttons go (for right-click to add Null Button)
function OnEmptyAnchorClick(id) {
    if (event.button == 2)
        window.external.OnAnchorButton(id);
}
function OnLineOptionsButton(btn) {
    // capture the last textarea selected before it loses focus to do a context menu
    TriggerMyBlur(true);

    var bIsRightButton = (event.button == 2);
    window.external.OnLineOptionsButton(btn.id, bIsRightButton);
    return false;
}
function OnVerseLineJump(link) {
    window.external.OnVerseLineJump(link.name);
    return false; // cause the href navigation to not happen
}
function textboxSetSelectionTextReturnEndPosition(strId, strNewValue) {
    var oTextbox = document.getElementById(strId);

    // if we had turned our selection into a span earlier, then convert it back
    if (oTextbox.selectedText) {
        var range = oTextbox.createTextRange();
        range.collapse(true);
        range.moveStart("character", oTextbox.selectionStart);
        range.moveEnd("character", oTextbox.selectionEnd - oTextbox.selectionStart);
        range.select();
    }

    var rangeSelection = document.selection.createRange();
    var rangeElement = rangeSelection.duplicate();
    rangeElement.moveToElementText(oTextbox);
    var nEndPoint = 0;
    if (rangeElement.inRange(rangeSelection)) {
        rangeSelection.text = strNewValue;
        rangeSelection.select();
        while (rangeElement.compareEndPoints('StartToEnd', rangeSelection) < 0) {
            rangeElement.moveStart('character', 1);
            nEndPoint++;
        }
    }
    return nEndPoint;
}

/*
function OnKeyDown() {
    // if this is F5 (refresh)...
    if (window.event.keyCode == 116) {
        // let the form handle it
        window.external.LoadDocument();

        // disable the propagation of the F5 event
        window.event.keyCode = 0;
        window.event.returnValue = false;
        return false;
    }
    /* this is for searching... not implemented yet
    else if (window.event.ctrlKey && (window.event.keyCode == 70)) {
        if (window.event.stopPropagation) {
            window.event.stopPropagation();
        }
        else {
            window.event.cancelBubble = true;
            window.event.returnValue = false;
            window.event.keyCode = 0;
        }
        window.external.DoFind();
        return false;
    }
    // /
    return true;
}
*/
function DisplayHtml(strFunction) {
    var debugWindow = $('#osedebughtmlwindow');
    if (debugWindow) {
        var curVal = debugWindow.val();
        $('#osedebughtmlwindow').val(curVal + " " + strFunction);
    }
}
function removeSelection(jqtextarea) {
    if (jqtextarea.attr('selectedText')) {
        jqtextarea.removeAttr("selectionStart");
        jqtextarea.removeAttr("selectionEnd");
        jqtextarea.removeAttr("selectedText");
        removeSpan(jqtextarea);    // also remove the highlighting
    }
}
function CheckRemovePlaceHolder(jqtextarea) {
    if (jqtextarea.attr('placeholder') != '' && jqtextarea.val() == jqtextarea.attr('placeholder')) {
        jqtextarea.val('').removeClass('hasPlaceholder');
    }
}
function removeSpan(jqtextarea) {
    jqtextarea.html(jqtextarea.val());
}

function ClearSelectionSpan(strId) {
    var oTextbox = document.getElementById(strId);
    removeSelection($(oTextbox));
}

// the following code used to be in onblur (and works just
//  fine that way in IE). But for some reason, in a
//  WebBrowser in a WindowsForm, the onblur is triggered
//  even when the textarea is still in focus. So to work
//  around this problem, what was in onblur is now called by
//  the onfocus handler (before getting to the work of the
//  textarea newly in focus) as well as by my WindowsForm app
//  if the WebBrowser itself loses focus (e.g. to go to some
//  other control in the app).
function TriggerMyBlur(bDontEmptySelection) {
    if (window.oseConfig.idLastTextareaToBlur) {
        var oldThis = document.getElementById(window.oseConfig.idLastTextareaToBlur);
        if (oldThis.selectedText) {
            var text = oldThis.value;
            var pre = (oldThis.selectionStart > 0)
                ? text.substring(0, oldThis.selectionStart)
                : "";
            var post = (oldThis.selectionEnd < text.length)
                ? text.substring(oldThis.selectionEnd)
                : "";

            // DisplayHtml("myblur: pre: " + pre + ", post: " + post + ", sel: " + oldThis.selectedText);
            DisplayHtml("myblur: sel: " + oldThis.selectedText);

            // if this is being called by the app, it's possible that the selection
            //  here is still selected. Then the process of replacing the 'html' is likely
            //  to cause all the text in the textarea to be selected. So before doing
            //  this replacement, go ahead and collapse the selected text. (but only
            //  if not triggered from this html (i.e. only if triggered from the app)
            if (!bDontEmptySelection)
                document.selection.empty();

            $(oldThis).html(pre + "<span class='" + oldThis.className + " highlight'>" + oldThis.selectedText + "</span>" + post);
        }
        window.oseConfig.idLastTextareaToBlur = null;
    }
}
$(document).ready(function () {
    $('textarea').select(function (event) {
        var range = document.selection.createRange();
        if (range.text.length > 0) {
            var storedRange = range.duplicate();
            storedRange.moveToElementText(this);
            storedRange.setEndPoint('EndToEnd', range);
            this.selectionStart = storedRange.text.length - range.text.length;
            this.selectionEnd = this.selectionStart + range.text.length;
            this.selectedText = range.text;
        }
        else {
            removeSelection($(this));
        }
        // DisplayHtml(event.type + this.selectedText);
    }).blur(function (event) {
        if ($(this).attr('placeholder') != '' && ($(this).val() == '' || $(this).val() == $(this).attr('placeholder'))) {
            $(this).val($(this).attr('placeholder')).addClass('hasPlaceholder');
        }
        window.oseConfig.idLastTextareaToBlur = this.id;
        window.external.TextareaOnBlur(this.id);
        DisplayHtml(event.type);
    }).focus(function (event) {
        CheckRemovePlaceHolder($(this));

        // for some reason, in the WebBrowser, we continue to get
        //  focus events even after the textarea is in focus...
        if (window.oseConfig.idLastTextareaToFocus == this.id)
            return;

        window.oseConfig.idLastTextareaToFocus = this.id;

        // for some reason, in IE, the following code works
        //  fine in onblur, but not in a WebBrowser in IE. So we have
        //  to trigger an onblur from here before dealing with focus
        //  this will turn the selected text from the last control
        //  to have focus into a span.
        TriggerMyBlur(true);

        // now if we previously added the span (what we do
        //  during TriggerMyBlur but for this control now)
        //  then it has to be removed when we begin editing
        //  this textarea again
        if ($(this).has("span").length) {
            // setting the html with only the value will
            //  remove the span element.
            removeSpan($(this));

            // and if we had previously a selected portion in
            //  this textarea, select it again
            if (this.selectedText) {
                var range = this.createTextRange();
                range.collapse(true);
                range.moveStart("character", this.selectionStart);
                range.moveEnd("character", this.selectionEnd - this.selectionStart);
                range.select();
            }
        }
        window.external.TextareaOnFocus(this.id);
        // DisplayHtml(event.type + ", id: " + this.id);
        DisplayHtml(event.type);
    }).mouseup(function (event) {
        if (event.button == 2)  // right-clicked...
        {
            // DisplayHtml("mu: lst2blr: " + window.oseConfig.idLastTextareaToBlur + ", this: " + this.id);
            // if the user right-clicks as the first event in a new textarea, it will get
            //  focus event first, which will turn it's span into a selection (which we
            //  don't want, because right-click means we're probably going to need the
            //  spans during the subsequent call to, e.g. add note on selected text).
            // So in the case of a right-click, undo the selection and go back to a span
            //  (whether or not we empty the selection first depends on whether this is
            //  a new textarea or not. If it's a new text area, then last2blur will be
            //  null and then bDontEmptySelection will be false which will trigger the
            //  collapse of the selection (not sure... but it works...)
            window.oseConfig.idLastTextareaToBlur = this.id;
            var bDontEmptySelection = !(window.oseConfig.idLastTextareaToBlur == this.id);
            TriggerMyBlur(bDontEmptySelection);

            // tell app to show the context menu for this control
            window.external.ShowContextMenu(this.id);
            // return false;
        }
        window.external.TextareaMouseUp(this.id);
        return true;
    }).mousedown(function (event) {
        CheckRemovePlaceHolder($(this));    // remove the place holder in case this is TextPaster (or the name of the languages is thought to be text)
        window.external.OnTextareaMouseDown(this.id, this.value, event.button);
        if (event.button == 1)
            removeSelection($(this));
    }).mousemove(function () {
        window.external.OnMouseMove();
    }).keyup(function (event) {
        // if we had something selected and the user presses delete or backspace,
        // UPDATE (10/5/13): if the user types *anything* after selecting, we need to remove the selection...
        // then we have to clear out the selection (so it doesn't reoccur if we trigger blur)
        $(this).removeAttr("selectedText");

        // then there are certain keys we don't want to trigger this for
        if (ctrl_down && ((event.keyCode == ctrl_key) ||   // ignore this one... (it's the control key)
                          (event.keyCode == c_key) ||   // copy
                          (event.keyCode == a_key) ||   // select all
                          (event.keyCode == f_key) ||   // find
                          (event.keyCode == h_key) ||   // replace
                          (event.keyCode == s_key))) {
            return true;
        }
        DisplayHtml("ctrl_down = " + ctrl_down + ", and event.keyCode = " + event.keyCode);
        return window.external.TextareaOnKeyUp(this.id, this.value);
    }).keydown(function (event) {
        if (ctrl_down && ((event.keyCode == v_key) ||   // paste
                          (event.keyCode == x_key))) {  // cut
            $(this).removeAttr("selectedText"); // cut or paste means we no longer have a selection
        }
        DisplayHtml(event.type + event.keyCode + this.value);
    }).dblclick(function (event) {
        var sel = document.selection;
        var rng = sel.createRange();
        rng.expand("word");
        rng.select();
    });
    $('.readonly').attr('readonly', 'readonly');
});
window.oseConfig =
{
    idLastTextareaToBlur: null,
    idLastTextareaToFocus: null
};

var ctrl_down = false;
var ctrl_key = 17;
var a_key = 65;
var c_key = 67;
var f_key = 70;
var h_key = 72;
var s_key = 83;
var v_key = 86;
var x_key = 88;
var f5_key = 116;

$(document).keydown(function (e) {
    if (e.keyCode == ctrl_key) ctrl_down = true;
}).keyup(function (e) {
    if (e.keyCode == ctrl_key) ctrl_down = false;
});

$(document).keydown(function (e) {
    if (ctrl_down && (e.keyCode == s_key)) {
        window.external.OnSaveDocument();
        // Your code
        e.preventDefault();
        return false;
    }
    else if (e.keyCode == f5_key) {
        if (ctrl_down) {
            window.external.TriggerCtrlF5();
        }

        // let the form handle it
        window.external.LoadDocument();
        // doesn't work... e.preventDefault();
        return true;
    }
});