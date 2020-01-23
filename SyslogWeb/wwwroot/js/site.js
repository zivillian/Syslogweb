$('.dropdown-menu').on('click', function (e) {
    if ($(this).hasClass('dropdown-menu-form')) {
        e.stopPropagation();
    }
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
    var port = $form.attr('data-wsport');
    var id = $form.attr('data-id');
    if (id === '000000000000000000000000') return;
    var connection = new WebSocket('ws://' + window.location.hostname + ':' + port + '/syslog');
    var $table = $('table.table > tbody');
    
    connection.onopen = function () {
        connection.send(JSON.stringify({ Id: id, Search: $form.attr('data-query') }));
    };
    
    connection.onmessage = function (e) {
        var data = JSON.parse(e.data);
        var template = '<tr class="{{CssClass}}"><td>{{Date}}</td>';
        if (data.HostAsLink) {
            template = template + '<td style="white-space: nowrap;"><a href="#" data-text=" host:{{Host}}">{{Host}}</a></td>';
        } else {
            template = template + '<td style="white-space: nowrap;">{{Host}}</td>';
        }
        template = template + '<td>{{Severity}}</td><td>{{Facility}}</td>';
        if (data.ProgramAsLink) {
            template = template + '<td><a href="#" data-text=" program:{{Program}}">{{Program}}</a></td>';
        } else {
            template = template + '<td>{{Program}}</td>';
        }
        template = template + '<td>{{Text}}</td></tr>';
        var row = Mustache.render(template, data);
        var $row = $(row);
        $row.find('a').click(function(ev) {
            var value = $(ev.target).attr('data-text');
            var $search = $('#searchinput');
            var text = $search.val();
            $search.val((text + value).trim());
            $('#searchform').submit();
        });
        $table.prepend($row);
    };

    $('#pause').on('click', function () {
        var $span = $(this).find('span');
        if ($span.hasClass('fa-pause')) {
            $span.removeClass('fa-pause').addClass('fa-play');
            connection.send('pause');
        } else {
            $span.removeClass('fa-play').addClass('fa-pause');
            connection.send('resume');
        }
    });
}

$(document).ready(function () {
    window.prettyPrint && prettyPrint();
    StartWebsocket();
});