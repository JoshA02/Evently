$(() => {
    $(".event").on("click", (e) => {
        window.location.href = "/events/view/" + e.currentTarget.id;
    });
});