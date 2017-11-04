import { inject } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';
import { viewEngineHooks } from 'aurelia-templating';

interface TableItem {
    data: any[];
    headers: string[];
    pageSize: number;
    totalItems: number;
    currentPage: number;
}


@inject(EventAggregator)
export class TableView {

    private tableItems: any[];
    private ea: EventAggregator;

        constructor(eventAggregator: EventAggregator) {
        this.ea = eventAggregator;
    }

    clear() {
        this.tableItems = [];
    }

    write(data) {
        var flattenedData = data.map(this.flattenGraphson);
        if (flattenedData.length > 0) {
            var headers = [];
            for (var key in flattenedData[0])
                headers.push(key);

            var tableItem = {};
            tableItem['data'] = flattenedData;
            tableItem['headers'] = headers;
            tableItem['currentPage'] = 1;
            tableItem['pageSize'] = 10;
            this.tableItems.push(tableItem);
        }
    }

    private value(item, property) {
        return item[property];
    }

    private flattenGraphson(data) {
        var res = {};
        switch (data.type) {
            case 'vertex':
                if (data.properties != null) {
                    for (var kk in data.properties) {
                        res[kk] = data.properties[kk][0].value;
                    }
                }
                break;

            case 'edge':
                if (data.properties != null) {
                    for (var i in data.properties) {
                        res[i] = data.properties[i];
                    }
                }
                break;

            //Value map
            default:
                Object.keys(data).map(function (id, index) {
                    res[id] = data[id];
                });

        }

        return res;
    }

    private downloadData(data) {
        var csvLines = data.map(function (d) {
            var str = [];
            for (var key in d)
                str.push(d[key]);
            return str.join(',');
        })

        var headerLine = [];
        for (var key in data[0])
            headerLine.push(key);
        csvLines.splice(0, 0, headerLine.join(','));

        var csv = csvLines.join('\n')

        if (navigator.msSaveOrOpenBlob) {
            // Works for Internet Explorer and Microsoft Edge
            var blob = new Blob([csv], { type: "text/csv" });
            navigator.msSaveOrOpenBlob(blob, "export.csv");
        }
        else {
            var a = document.createElement('a');
            a.style.display = 'none';
            a.download = "export.csv";
            document.body.appendChild(a);
            a.href = encodeURI("data:text/csv;charset=utf-8," + csv);
            a.click();
            a.remove();
        }
    }
}


@viewEngineHooks()
export class TableViewBinder {
    beforeBind(view) {
       
    }
}