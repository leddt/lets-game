import tippy from "tippy.js";
import "tippy.js/dist/tippy.css";

/**
 * @param {HTMLElement} node
 * @param {string} text
 */
export default function (node, text) {
  let tippyInstance;

  update(text);

  return {
    update,
    destroy
  };

  function update(text) {
    if (text) {
      setText(text);
    } else {
      destroy();
    }
  }

  function setText(text) {
    if (tippyInstance) {
      tippyInstance.setContent(text);
    } else {
      tippyInstance = tippy(node, { content: text });
    }
  }

  function destroy() {
    if (tippyInstance) {
      tippyInstance.destroy();
    }

    tippyInstance = null;
  }
}
