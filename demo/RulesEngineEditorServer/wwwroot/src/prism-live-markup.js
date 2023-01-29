{

let _ = Prism.Live;

_.registerLanguage("markup", {
	comments: {
		multiline: ["<!--", "-->"]
	},
	selfClosing: ["input", "img", "link", "meta", "base", "br", "hr"],
	snippets: {
		"submit": '<button type="submit">Submit</button>',
		custom: function (selector) {
			var isName = /^[\w:-]+$/.test(selector);
			var isSnippet = isName || selector.match(/^[.#\w:-]+(\{.+?\})?(\*\d+)?$/);
			var node = this.getNode();
			var inTag = node.parentNode.closest(".token.tag");

			if (isName && inTag) {
				// Attribute
				return `${selector}="$1"`;
			}
			else if (isSnippet) {
				var times = 1;
				var content = "";

				if (isSnippet[1]) {
					// Content
					content = isSnippet[1].slice(1, -1);
				}

				if (isSnippet[2]) { // Times
					times = isSnippet[2].slice(1);
				}

				var tag = _.match(selector, /^[\w:-]+/, "div");
				var html = `<${tag}`;
				var id = _.match(selector, /#([\w-]+)/, 1);

				if (id) {
					html += ` id="${id}"`;
				}

				var classes = selector.match(/\.[\w-]+/g);

				if (classes) {
					classes = classes.map(x => x.slice(1));
					html += ` class="${classes.join(" ")}"`;
				}

				var selfClosing = _.languages.markup.selfClosing.indexOf(tag) > -1;

				html += selfClosing? "$1 />$2" : ">$1";

				var tagLength = html.length;
				var ret = "";

				for (var i=0; i<times; i++) {
					// Tag
					if (selfClosing) {
						ret += html;
					}
					else {
						ret += `${html}${content}</${tag}>`;
					}

					if (times > 1 && i + 1 < times) {
						ret += "\n";
					}
				}

				return ret;
			}
		}
	}
});

}
