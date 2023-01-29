/*
 * JS for Prism Live’s page, not part of the actual editor
 */

(async function($, $$) {


var js = await fetch("src/prism-live.js");
js = await js.text();

$$("textarea.language-js.fill").forEach(t => {
	t.value = js;
	t.dispatchEvent(new InputEvent("input"));
});


})(Bliss, Bliss.$);
