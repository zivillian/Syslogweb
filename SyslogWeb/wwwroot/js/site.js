$('.dropdown-menu').on('click', function (e) {
    e.stopPropagation();
});

$('#facilityform input[type=checkbox]').change(function () {
    var $this = $(this);
    var $search = $('#searchinput');
    var text = $search.val();
    if (this.checked) {
        $search.val(text + ' facility:' + $this.val());
    } else {
        $search.val(text.replace('facility:' + $this.val(), '').replace('facility:!' + $this.val(), '').replace('/\s+/g', ' ').trim());
    }
    $('#searchform').data('changed', true);
});

$('#facilityform').on('hidden.bs.dropdown', function () {
    if ($('#searchform').data('changed'))
        $('#searchform').submit();
});

$('#severityform').on('hidden.bs.dropdown', function () {
    if ($('#searchform').data('changed'))
        $('#searchform').submit();
});

$('#severityform input[type=checkbox]').change(function () {
    var $this = $(this);
    var $search = $('#searchinput');
    var text = $search.val();
    if (this.checked) {
        $search.val(text + ' severity:' + $this.val());
    } else {
        $search.val(text.replace('severity:' + $this.val(), '').replace('severity:!' + $this.val(), '').replace('/\s+/g', ' ').trim());
    }
    $('#searchform').data('changed', true);
});

//$('.input-group.date').datetimepicker({ useCurrent: false, language: 'de' })
//    .on('dp.hide', function() {
//        if ($('#searchform').data('changed'))
//            $('#searchform').submit();
//    }).on('dp.change', function () {
//        $('#searchform').data('changed', true);
//    });

function StartWebsocket() {
    var $form = $('#searchform');
    var id = $form.attr('data-id');
    if (id === '000000000000000000000000') return;
    var connection = new signalR.HubConnectionBuilder().withUrl("/log").build();
    var $table = $('table.table > tbody');

    $('#pause').on('click', function () {
        var $span = $(this).find('span');
        if ($span.hasClass('fa-pause')) {
            $span.removeClass('fa-pause').addClass('fa-play');
            connection.invoke('Pause');
        } else {
            $span.removeClass('fa-play').addClass('fa-pause');
            connection.invoke('Resume');
        }
    });
    connection.start().then(function() {
        connection.stream("Tail", { Id: id, Search: $form.attr('data-query') } )
        .subscribe({
            next: (item) => {
                var template = `<tr class="${item.cssClass}"><td>${item.date}</td>`;
                if (item.hostAsLink) {
                    template = template + `<td style="white-space: nowrap;"><a href="#" data-text=" host:${item.host}">${item.host}</a></td>`;
                } else {
                    template = template + `<td style="white-space: nowrap;">${item.host}</td>`;
                }
                template = template + `<td>${item.severity}</td><td>${item.facility}</td>`;
                if (item.programAsLink) {
                    template = template + `<td><a href="#" data-text=" program:${item.program}">${item.program}</a></td>`;
                } else {
                    template = template + `<td>${item.program}</td>`;
                }
                template = template + `<td>${item.text}</td></tr>`;
                var row = template;
                var $row = $(row);
                $row.find('a').click(function(ev) {
                    var value = $(ev.target).attr('data-text');
                    var $search = $('#searchinput');
                    var text = $search.val();
                    $search.val((text + value).trim());
                    $('#searchform').submit();
                });
                $table.prepend($row);
            },
            complete: () => {
                console.log('log completed???');
            },
            error: (err) => {
                console.log(err);
            }
        });
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

$(document).ready(function () {
    StartWebsocket();
});