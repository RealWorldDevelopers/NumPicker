

$(document).ready(function () {

    $('body').off('click', '#btnAdd');
    $('body').on('click', '#btnAdd', function () {

        // get picks
        var arr = $('select').map(function () {
            return this.value;
        }).get();

        // check each ball has a selected value
        var allSelected = allSelectionsMade(arr);
        if (allSelected) {

            // stage array for main picks
            var dupeArray = Array.from(arr);

            // handle if solo ball needs removed from arr before sending...
            var title = document.getElementsByTagName('title')[0].innerHTML;
            var hasMegaBall = title.indexOf('Mega') !== -1;
            var hasPowerBall = title.indexOf('Power') !== -1;

            if (hasMegaBall || hasPowerBall) {
                // alert('pop');
                dupeArray.pop();
            }

            // check for dupes and add pick to display
            var hasDupes = hasDuplicates(dupeArray);
            if (!hasDupes) {
                addPick(arr);
            }

        }


    });

    
    function validatePick(ball) {
        return ball <= 0;
    }

    function allSelectionsMade(arr) {
        var allPicksSelected = arr.findIndex(validatePick);
        // alert(allPicksSelected);
        if (allPicksSelected < 0) {
            return true;
        } else {
            alert('Invalid Pick - Missing Ball Value');
            return false;
        }
    }

    function hasDuplicates(arr) {
        // check for dupes
        var hasDups = !arr.every(function (v, i) {
            return arr.indexOf(v) === i;
        });
        if (hasDups) {
            alert('Invalid Pick - Duplicates');
            return true;
        } else {
            return false;
        }
    }

    function addPick(arr) {
        var table = $('#pickTableBody');     

        table.append('<tr>');

        arr.forEach(function (item) {
            table.append('<td>' + item + '</td>');
        });

        table.append('</tr>');
    }

});
