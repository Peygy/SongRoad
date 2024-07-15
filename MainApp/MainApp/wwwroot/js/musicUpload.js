document.addEventListener("DOMContentLoaded", function () {
    var trackItems = document.querySelectorAll(".track-item");

    trackItems.forEach(function (item) {
        var trackId = item.id.split('-')[1];

        var editLink = document.createElement("a");
        editLink.href = 'update/' + trackId;
        editLink.textContent = "Редактировать";

        var deleteLink = document.createElement("a");
        deleteLink.href = "javascript:void(0);";
        deleteLink.textContent = "Удалить";
        deleteLink.addEventListener("click", function () {
            deleteTrack(trackId);
        }, false);

        item.appendChild(editLink);
        item.appendChild(deleteLink);
    });
});
function deleteTrack(trackId) {
    if (confirm('Вы уверены, что хотите удалить этот трек?')) {
        fetch(`/user/tracks/delete/${trackId}`, {
            method: 'DELETE'
        }).then(response => response.json())
            .then(data => {
                if (data.success) {
                    document.getElementById(`track-${trackId}`).remove();
                } else {
                    alert('Ошибка при удалении трека');
                }
            })
            .catch(error => console.error('Error:', error));
    }
}