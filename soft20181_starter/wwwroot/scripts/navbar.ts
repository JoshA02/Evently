$(() => {
    const body = $("body");
    const lightbulbButton = $("#toggle-appearance");

    const systemColourScheme: string = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
        ? "dark"
        : "light"
    ;

    // Set initial appearance
    const setLightbulbGlow = () => {
        let currentAppearance = localStorage.getItem("colour-scheme")?.toLowerCase() || "light";
        if(currentAppearance == "system") currentAppearance = systemColourScheme;
        if(currentAppearance.toLowerCase() === "light") $(lightbulbButton).addClass("active");
    };


    // Highlight current page; default to index.html if they're on the root
    const highlightCurrentPageTab = () => {
        // let currentPage = window.location.pathname.split("/").pop() || "index";
        // currentPage = currentPage.split(".")[0].toLowerCase(); // Remove file extension and convert to lowercase
        let currentPage = window.location.pathname.split("/")[1];
        if (currentPage == "") currentPage = "index";
        if (currentPage == "event-detail") currentPage = "events";
        $(`.navbar-links > a[href="/${currentPage}"]`).addClass("active");
    };


    const setInitialAppearanceButtons = () => {
        const onPopup: boolean = !(window.location.pathname.split("/").pop() == "Appearance");

        const inputNames = ["font-size", "colour-scheme", "font-family"];
        const defaults = ["medium", "light", "default"];

        inputNames.forEach((inputNames, index) => {
            let currentProp = localStorage.getItem(inputNames) || defaults[index];
            if(!onPopup) currentProp += "-page"; // Add "-page" to the end of the selector if we're on the appearance page
            $(`input[name="${inputNames}"][id="${currentProp}"]`).prop("checked", true);
        });
    };

    // Change Appearance (Radio Buttons)
    $(".option > input").on("change", (event) => {
        const target = $(event.target);
        const name = target.attr("name");
        let value = target.attr("id");
        if(value == undefined) return;

        value = value.split("-page")[0]; // Remove "-page" from the end of the selector if we're on the appearance page

        switch(name) {
            case "font-size":
                updateFontSize(value);
                break;
            case "colour-scheme":
                updateColourScheme(value);
                break;
            case "font-family":
                updateFontFamily(value);
                break;
        }
    });

    const updateFontFamily = (newFamily: string) => {
        newFamily = newFamily.toLowerCase();
        localStorage.setItem("font-family", newFamily);

        $(body).removeClass("font-1");
        $(body).removeClass("font-2");
        if(newFamily == "font-1" || newFamily == "font-2") $(body).addClass(newFamily);
    }

    const updateFontSize = (newSize: string) => {
        newSize = newSize.toLowerCase();
        $(body).removeClass("small");
        $(body).removeClass("medium");
        $(body).removeClass("large");
        $(body).addClass(newSize);

        localStorage.setItem("font-size", newSize);
    };

    const updateColourScheme = (newScheme: string) => {
        newScheme = newScheme.toLowerCase();
        localStorage.setItem("colour-scheme", newScheme); // Update local storage (light, dark, system)

        if(newScheme == "system") newScheme = systemColourScheme;

        if(newScheme == "dark") {
            $(body).addClass("dark");
            $(lightbulbButton).removeClass("active");
            return;
        }
        $(body).removeClass("dark");
        $(lightbulbButton).addClass("active");
    };


    $('.logo').on('click', () => {
        window.location.href = '/index';
    });

    const onSearch = (event: JQuery.SubmitEvent<HTMLElement, undefined, HTMLElement, HTMLElement>) => {
        event.preventDefault();
        const form = $(event.target);
        const search = form.children('input').val();
        if(search == undefined) return;
        
        const queryParams = new URLSearchParams(window.location.search);
        queryParams.set('q', search.toString());
        window.location.href = `/events?${queryParams.toString()}`;
    };

    const setShowMobileSearch = (show: boolean) => {
        const form = $(".mobile-search-bar-container");
        if(show) {
            form.slideToggle(500);
            $("#mobile-search").text("close");
        } else {
            form.slideToggle(500);
            $("#mobile-search").text("search");
        }
    };

    const setShowMobileSidebar = (show: boolean) => {
        const sidebar = $(".navbar-side");
        if(show) {
            sidebar.animate({width: 'show'}, 500);
            $("#burger-menu").text("close");
        } else {
            sidebar.animate({width: 'hide'}, 500);
            $("#burger-menu").text("menu");
        }
    };

    const setShowPersonalisationMenu = (show: boolean) => {
        const menu = $(".personalisation-menu");
        if(show) {
            $(lightbulbButton).text("close");
            menu.animate({width: 'show'}, 500);
        } else {
            $(lightbulbButton).text("lightbulb");
            menu.animate({width: 'hide'}, 500);
        }
    };

    // Toggle mobile sidebar
    $("#burger-menu").on("click", () => {
        const sidebar = $(".navbar-side");
        const isOpen = sidebar.css("display") == "none";
        setShowMobileSidebar(isOpen);
    });

    // Toggle mobile search bar
    $("#mobile-search").on("click", () => {
        const form = $(".mobile-search-bar-container");
        const isOpen = form.css("display") == "none";
        setShowMobileSearch(isOpen);
    });

    $(lightbulbButton).on("click", () => {
        const menu = $(".personalisation-menu");
        const isOpen = menu.css("display") == "none";
        setShowPersonalisationMenu(isOpen);
    });

    $(".navbar-side").hide(); // Hide mobile sidebar by default
    $(".mobile-search-bar-container").hide(); // Hide mobile search bar by default
    $(".personalisation-menu").hide(); // Hide personalisation menu by default

    // Add event listeners to search forms
    $("#search-form-mobile").on("submit", (event) => onSearch(event));
    $('#search-form').on('submit', (event) => onSearch(event));

    // Set initial appearance of radio buttons (personalisation menu)
    setInitialAppearanceButtons();

    // Set initial appearance of the lightbulb icon
    setLightbulbGlow();

    // Highlight the tab for the current page
    highlightCurrentPageTab();
});