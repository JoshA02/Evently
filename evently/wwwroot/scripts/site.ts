$(() => {
    
    const body = $("body");
    
    
    // Fetch appearance preference from local storage
    function setInitialAppearance() {
        if(localStorage.getItem("colour-scheme") === null) localStorage.setItem("colour-scheme", "light");
        if(localStorage.getItem("font-size") === null) localStorage.setItem("font-size", "medium");
        if(localStorage.getItem("font-family") === null) localStorage.setItem("font-family", "default");

        const systemColourScheme: string = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
            ? "dark"
            : "light"
        ;

        let newScheme = localStorage.getItem("colour-scheme")?.toLowerCase() || "light";
        if(newScheme == "system") newScheme = systemColourScheme;

        const isDark = newScheme == "dark";

        if(isDark) body.addClass("dark");
        else body.removeClass("dark");

        const storedFontSize = localStorage.getItem("font-size")?.toLowerCase() || "medium";
        const fontSize = ((storedFontSize === "small" || storedFontSize === "medium" || storedFontSize === "large") ? storedFontSize : "medium" );
        $(body).removeClass("small");
        $(body).removeClass("medium");
        $(body).removeClass("large");
        $(body).addClass(fontSize);

        const storedFontFamily = localStorage.getItem("font-family")?.toLowerCase() || "default";
        const fontFamily = ((storedFontFamily === "font-1" || storedFontFamily === "font-2") ? storedFontFamily : "default" );
        $(body).removeClass("font-1");
        $(body).removeClass("font-2");
        if(fontFamily != "default"){
            $(body).addClass(fontFamily);
        }
        
        body.css("display", "unset");
    }
    setInitialAppearance();
});