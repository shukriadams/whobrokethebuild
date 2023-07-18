// strips markup whitespace from code elements and all children. This method is a bit overly aggressive, but there's no other way of remove whitespace
(() => {
    function trimWhiteSpace(codeElement) {
        // trim self
        codeElement.innerHTML = codeElement.innerHTML.trim()
        // trim space between children
        codeElement.innerHTML = codeElement.innerHTML.replaceAll(/>\s+</g, '><')
        for (let child of codeElement.children) 
            trimWhiteSpace(child)
    }

    for (let codeElement of document.querySelectorAll('.code')) 
        trimWhiteSpace(codeElement)
    
})()