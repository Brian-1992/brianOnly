/* PostForm:送出表單
   用法: PostForm(url, paramsArr);
   人員: 江吉威 2018/12/3           */
PostForm = function (url, p) {
    var f = $('<form/>').attr('action', url).attr('method', 'POST');
    if (p) {
        for (i = 0; i < p.length; i++)
            f.append($('<input/>').attr('name', escapeHtml(p[i].name)).attr('value', escapeHtml(p[i].value)));
    }
    f.appendTo('body').submit().remove();
}
/* DownloadFile:下載檔案(可用於超連結)
   用法: DownloadFile(file_guid);
   人員: 江吉威 2018/10/15           */
DownloadFile = function (fg) {
    var f = $('<input/>').attr('name', 'FG').attr('value', fg);
    $('<form/>').attr('action', '../../../api/File/Download')
        .attr('method', 'POST').append(f)
        .appendTo('body').submit().remove();
}
DownloadFileNA = function (fg) {
    var f = $('<input/>').attr('name', 'FG').attr('value', fg);
    $('<form/>').attr('action', '../../../api/NAFile/Download')
        .attr('method', 'POST').append(f)
        .appendTo('body').submit().remove();
}

/* showUploadWindow:顯示上傳/下載視窗元件
   用法: showUploadWindow(true(可上傳), true(可刪除), upload_key, title, viewport, [p](其他參數));
   人員: 江吉威 2018/10/09               */
function showUploadWindow(ua, da, uk, title, viewport, p) {
    var ec = "";
    if (p) { if (p.EC) ec = p.EC; }
    var lk = false;
    if (p) { if (p.LK) lk = p.LK; }
    var strUrl = "../UR/UR1999?UA=" + (ua ? 1 : 0) + "&DA=" + (da ? 1 : 0) + "&UK=" + uk + "&EC=" + ec + "&LK=" + (lk ? 1 : 0);
    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            id: 'iframeReport',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
            buttons: [{ xtype: 'hidden', id: 'CheckDesc' }, { xtype: 'label', id: 'DivResult', text: '上傳結果', width: '50%', color: 'blue' }, { xtype: 'hidden', id: 'args' },
            {
                id: 'winclosed',
                disabled: false,
                text: '關閉',
                handler: function () {
                    var args = $('#args')[0];
                    if (p) {
                        if (p.CD) {
                            if ($('#CheckDesc')[0].callback()) {
                                var w = Ext.Msg.show({
                                    title: '欄位輸入提示',
                                    msg: '<span style=\'color:red\'>檔案說明</span>不可空白 。',
                                    buttons: Ext.Msg.OK,
                                    icon: Ext.MessageBox.WARNING,
                                    cls: 'warnMsg'
                                });
                                return;
                            }
                        }
                    }
                    if (p && p.LK) {
                        var w = Ext.Msg.show({
                            title: '欄位輸入確認',
                            msg: '關閉視窗後，已輸入的<span style=\'color:red\'>檔案說明</span>將不能再編輯<br/>確定關閉？',
                            buttons: Ext.Msg.YESNO,
                            icon: Ext.MessageBox.WARNING,
                            cls: 'warnMsg',
                            fn: function (text, btn) {
                                if (text == 'no')
                                    return;
                                else {
                                    popform.up('window').destroy();
                                    if (p) { if (p.closeCallback) p.closeCallback(args.files); }
                                }
                            }
                        });
                    }
                    else {
                        this.up('window').destroy();
                        if (p) { if (p.closeCallback) p.closeCallback(args.files); }
                    }
                }
            }]
        });
        var win = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
    }
    win.show();
}
function showUploadWindowNA(ua, da, uk, title, viewport, p) {
    var ec = "";
    if (p) { if (p.EC) ec = p.EC; }
    var strUrl = "../../Show/UR/UR2999?UA=" + (ua ? 1 : 0) + "&DA=" + (da ? 1 : 0) + "&UK=" + uk + "&EC=" + ec;
    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            id: 'iframeReport',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
            buttons: [{ xtype: 'label', id: 'DivResult', text: '上傳結果', width: '50%', color: 'blue' },
            {
                id: 'winclosed',
                disabled: false,
                text: '關閉',
                handler: function () {
                    this.up('window').destroy();
                    if (p) { if (p.closeCallback) p.closeCallback(); }
                }
            }]
        });
        var win = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
    }
    win.show();
}
/*
  用法: showLogWindow('KONZS', '11004','','異動記錄',viewport);
  (1)TABLE_KEY：表格KEY=>KONZS。
  (2)TABLE_KEY_VALUE：表格KEY值,如供應商群組ID=>11004。
  (3) FIELD_NAME：非必要參數，如需指定只查某欄位異動記錄，才需提供，欄位名稱=>NAME1
*/
function showLogWindow(table_key, table_key_value, field_name, title, viewport) {
    var strUrl = "../../Show/UR/UR1010?TABLE_KEY=" + table_key + "&TABLE_KEY_VALUE=" + table_key_value + "&FIELD_NAME=" + field_name;
    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            id: 'iframeReport',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
            buttons: [
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
        });
        var win = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
    }
    win.show();
}
/* loadToDoCount:載入MenuItem旁的代辦事項數量
   用法: loadToDoCount('PR01', 25);
   人員: 江吉威 2018/10/03               */
function loadToDoCount(node_id, count) {

    var store;
    if (typeof MenuStore != 'undefined') store = MenuStore;
    if (typeof parent.MenuStore != 'undefined') store = parent.MenuStore;
    if (!store) { alert('無法更新功能目錄'); return; }
    var node = store.getNodeById(node_id);


    if (node) {

        var ot = node.data.text;

        var ind = ot.indexOf(" ");
        if (ind != -1)
            ot = ot.substring(0, ind);
        node.data.text = ot + " (<span style='color:red'>" + count + "</span>)";
        if (node)
            store.load({ node: node });

    }


}

/* 此函式用於處理dirty頁面跳出時，顯示是否離開提示訊息，加入人員 612764_3410 陳思評 2013/02/04 */
function my_onbeforeunload(e) {
    message = '有尚未儲存的資料，確定要離開嗎?';
    e = e || window.event;
    if (e) {
        e.returnValue = message;
    }
    return message;
}
/* 此函式用於將訊息拋至訊息區，加入人員 612764_3410 陳思評 2013/02/04 */
function msglabel(msg) {
    if (!msg.success) {
        if (msg.indexOf('訊息區:') > -1) msg = msg.replace('訊息區:', '');
        var lbl = $('#msglabel', top.document)[0];
        if (typeof (lbl) !== "undefined" && lbl !== null) {
            $('#msglabel', top.document)[0].innerHTML = msg;
        }
    }
    else {
        var _icon = "[錯誤]";
        if (msg.success) _icon = "[成功]";
        var _msg = "";
        if (msg.msg) _body = msg.msg;
        var lbl = $('#msglabel', top.document)[0];
        if (typeof (lbl) !== "undefined" && lbl !== null) {
            $('#msglabel', top.document)[0].innerHTML = _icon + _msg;
        }
    }
}
/* 避免使用者按下倒退鍵產生回到上一頁，加入人員 612764_3410 陳思評 2013/03/18 */
Ext.getDoc().on('keydown', function (e) {
    if (e.getKey() == 8 && e.getTarget().type == 'text' && !e.getTarget().readOnly) {

    } else if (e.getKey() == 8 && e.getTarget().type == 'textarea' && !e.getTarget().readOnly) {

    } else if (e.getKey() == 8) {
        e.preventDefault();
    }

});
/* 傳入FG下載檔案，加入人員 612764_3410 陳思評 2013/04/11 */
function getFileName(fg) {
    window.open('../../../Download/Index/' + fg);
}

// 順序性載入js功能，加入人員 612670_2682 黃禹翔 2013/04/22
// 使用方法 loadScript.load(["a.js", "b.js"]);
var loadScript = (function () {
    var loadOne = function (url) {
        var dtd = $.Deferred();
        var node = document.createElement('script');
        node.type = "text/javascript";
        var onload = function () {
            dtd.resolve();
        };
        $(node).load(onload).bind('readystatechange', function () {
            if (node.readyState == 'loaded') {
                onload();
            }
        });
        document.getElementsByTagName('head')[0].appendChild(node);
        node.src = url;
        return dtd.promise();
    };
    var load = function (urls) {
        if (!$.isArray(urls)) {
            return load([urls]);
        }
        var ret = [];
        for (var i = 0; i < urls.length; i++) {
            ret[i] = loadOne(urls[i]);
        };
        return $.when.apply($, ret);
    }
    return {
        load: load
    };
});


/* 顯示訊息於標頭，加入人員 612764_3410 陳思評 2012/12/05 */
MsgTip = function () {
    var msgCt;
    function createBox(t, s) {
        return ['<div class="msg" >',
            //'<div class="x-box-tl"><div class="x-box-tr"><div class="x-box-tc"></div></div></div>',
            '<div class="x-box-ml"><div class="x-box-mr"><div class="TRAmsg-box">', t, s, '</div></div></div>',
            ,
            '</div>'].join('');
    }
    return {
        msg: function (title, message, autoHide, pauseTime) {
            if (!msgCt) {
                msgCt = Ext.DomHelper.insertFirst(document.body, { id: 'msg-div22', style: 'position:absolute;top:10px;width:100%;margin:0 auto;z-index:20000;' }, true);
            }
            msgCt.alignTo(document, 't-t');
            message += '<span style="text-align:right;font-size:12px;width:100%;margin-left:-200px;position:absolute;">' +
                '<font><u style="cursor:hand;text-decoration: none;" onclick="MsgTip.hide(this);">x</u></font></span>'
            var m = Ext.DomHelper.append(msgCt, { html: createBox(title, message) }, true);
            m.slideIn('t');
            if (!Ext.isEmpty(autoHide) && autoHide == true) {
                if (Ext.isEmpty(pauseTime)) {
                    pauseTime = 5;
                }
                m.pause(pauseTime).ghost("tr", { remove: true });
            }
        },
        hide: function (v) {
            var msg = Ext.get(v.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement);
            msg.ghost("tr", { remove: true });
        }
    };
}();

/* 日期驗證，加入人員 612764_3410 陳思評 2012/12/05 */
Ext.apply(Ext.form.field.VTypes, {
    dateRange: function (val, field) {
        if (field.dateRange) {
            if (field.dateRange.begin) {
                var beginId = field.dateRange.begin;
                this.beginField = Ext.getCmp(beginId);
                var beginDate = this.beginField.getValue();
                field.setMinValue(beginDate);
            }
            if (field.dateRange.end) {
                var endId = field.dateRange.end;
                this.endField = Ext.getCmp(endId);
                var endDate = this.endField.getValue();
                field.setMaxValue(endDate);
            }
            return true;
        }
        return false;
    },
    dateBetween: function (val, field) {
        //日期區間範圍設定 fix by Chilun 2013/09/02
        // A <= B <= C
        var date = field.parseDate(val);
        if (!date) {
            return false;
        }
        // C (or B)  結束日期檢查 --> 追溯開始日期
        if (field.startDateField && (!field.dateRangeMax || (date.getTime() != field.dateRangeMax.getTime()))) {
            var start = field.up('form').getForm().findField(field.startDateField);
            if (start.vtype != 'dateBetween') {
                start.setMaxValue(null);
            }
            else {
                start.setMaxValue(date);
            }
            //start.validate();
            field.dateRangeMax = date;
        }
        // A (or B)  開始日期檢查 --> 追溯結束日期
        if (field.endDateField && (!field.dateRangeMin || (date.getTime() != field.dateRangeMin.getTime()))) {
            var end = field.up('form').getForm().findField(field.endDateField);
            if (end.vtype != 'dateBetween') {
                end.setMinValue(null);
            }
            else {
                end.setMinValue(date);
            }
            //end.validate();
            field.dateRangeMin = date;
        }
        return true;
    },
    //數值區間範圍驗證 add by Chilun 2013/08/21
    numRange: function (val, field) {
        if (!val) {
            return;
        }
        if (field.startNumberField && (!field.numberRangeMax || (val != field.numberRangeMax))) {
            var start = field.up('form').getForm().findField(field.startNumberField); //Ext.getCmp(field.startNumberField);
            if (start) {
                start.setMaxValue(val);
                field.numberRangeMax = val;
                start.validate();
            }
        } else if (field.endNumberField && (!field.numberRangeMin || (val != field.numberRangeMin))) {
            var end = field.up('form').getForm().findField(field.endNumberField);  //Ext.getCmp(field.endNumberField);
            if (end) {
                end.setMinValue(val);
                field.numberRangeMin = val;
                end.validate();
            }
        }
        return true;
    },
    ////IP位址驗證 add by Chilun 2013/08/22
    //IPAddress: function (v) {
    //    return /^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$/.test(v);
    //},
    //IPAddressText: 'Must be a numeric IP address',
    //IPAddressMask: /[\d\.]/i,

    ////檔案格式驗證 add by Chilun 2013/08/22
    //file: function (val, field) {
    //    var fileName = /^.*\.(xlsx|xls)$/i;
    //    return fileName.test(val);
    //},
    //fileText: "File must be Microsoft Excel",
    //fileMask: /[a-z_\.]/i,

    //僅有數字驗證 add by Chilun 2013/08/22
    natural: function (val, field) {
        var reg = /^\d+$/i;
        return reg.test(val);
    },
    naturalText: '必須是純數字.',
    naturalMask: /^[0-9]/i
});

//日期區間範圍Min設定 fix by Chilun 2013/09/02
function dateRangeMIN(f) {
    var start = f.up('form').getForm().findField(f.startDateField);
    while (start.value == null) {
        start = f.up('form').getForm().findField(start.startDateField);
        if (start == null) { break; }
        if (start.startDateField) {
            break;
        }
    }
    if (start != null) {
        var dateS = f.parseDate(start.value);
        f.setMinValue(dateS);
        f.dateRangeMin = dateS;
    }
    else {
        f.setMinValue(null);
        //f.dateRangeMin = null;
    }
}
//日期區間範圍Max設定 fix by Chilun 2013/09/02
function dateRangeMAX(f) {
    var end = f.up('form').getForm().findField(f.endDateField);
    while (end.value == null) {
        end = f.up('form').getForm().findField(end.endDateField);
        if (end == null) { break; }
        if (end.endDateField) {
            break;
        }
    }
    if (end != null) {
        var dateE = f.parseDate(end.value);
        f.setMaxValue(dateE);
        f.dateRangeMax = dateE;
    }
    else {
        f.setMaxValue(null);
        //f.dateRangeMax = null;
    }
}
//修正數值在小數會進位的問題 fix by Chilun 2013/09/14
function numberValueFix(t) {
    var v = t.getRawValue();
    if (v) {
        var pos = (v.toString().indexOf('.'));
        if (pos > 0) {
            var s = v.toString().substring(pos + 1);
            if (s.length > t.decimalPrecision) {
                var f = Math.pow(10, t.decimalPrecision);
                v1 = Math.floor(v * f) / f;
                t.setRawValue(v1);
            }
        }
        else {
            t.setRawValue(v);
        }
    }
}
//設定值的區間範圍驗證 fix by Tohow 2013/09/14, fix by Chilun 2014/04/10,2014/06/26
function PCCT_setRange(f) {
    var v;
    var val = f.value;
    var minFields = f.minFields;
    var maxFields = f.maxFields;
    if (val != null) {      // fix when val is not null, by Chilun 2014/04/10
        if (minFields != "") {
            var mins = minFields.split(",");

            for (var i = 0; i < mins.length; i++) {
                v = f.up('form').getForm().findField(mins[i]);
                //v = f.up('form').down('datefield[name=mins[i]]');
                v.setMinValue(val);
                v.isValid();
                v.on('expand', function () {      //設定預設值 by David Huang 2014/12/01
                    PCCT_setDefValue(this);
                }, v, {
                        single: true //僅註冊一次
                    });
            }
        }
        if (maxFields != '') {
            var maxs = maxFields.split(",");
            for (var i = 0; i < maxs.length; i++) {
                v = f.up('form').getForm().findField(maxs[i]);
                //v = f.up('form').down('datefield[name=maxs[i]]');
                v.setMaxValue(val);
                v.isValid();
                v.on('expand', function () {      //設定預設值 by David Huang 2014/12/01
                    PCCT_setDefValue(this);
                }, v, {
                        single: true //僅註冊一次
                    });
            }
        }

    } else {    // fix when val is null, by Chilun 2014/06/26
        try {
            if (minFields != "") {
                var mins = minFields.split(",");

                for (var i = 0; i < mins.length; i++) {
                    v = f.up('form').getForm().findField(mins[i]);
                    v.setMinValue('');
                    v.isValid();
                }
            }
            if (maxFields != '') {
                var maxs = maxFields.split(",");
                for (var i = 0; i < maxs.length; i++) {
                    v = f.up('form').getForm().findField(maxs[i]);
                    v.setMaxValue('');
                    v.isValid();
                }
            }
        } catch (e) {

        }
    }
}

//DateField控件點擊預設日期為MinValue或MaxValue(Min優先) ,傳入控件本身     by David Huang 2014/11/18,2014/12/01,2015/3/13
function PCCT_setDefValue(f) {
    if ((f.maxValue) && (!f.minValue)) {
        if (f.value > f.maxValue) {
            f.setValue(f.maxValue);
        }
    } else {
        if (f.value < f.minValue) {
            f.setValue(f.minValue);
        }
    }
}

//設定值的區間範圍驗證(不等於) fix by Chilun 2014/04/10,2014/06/26
function PCCT_setRange2(f) {
    var v;
    var val = f.value;
    var minFields = f.minFields;
    var maxFields = f.maxFields;

    if (val != null) {
        if (minFields != "") {
            var mins = minFields.split(",");

            for (var i = 0; i < mins.length; i++) {
                v = f.up('form').getForm().findField(mins[i]);
                var minDate = new Date(new Date(val).setDate(val.getDate() + 1));
                v.setMinValue(minDate);
                v.isValid();
                v.on('expand', function () {      //設定預設值 by David Huang 2014/12/01
                    PCCT_setDefValue(this);
                }, v, {
                        single: true //僅註冊一次
                    });
            }
        }
        if (maxFields != '') {
            var maxs = maxFields.split(",");
            for (var i = 0; i < maxs.length; i++) {
                v = f.up('form').getForm().findField(maxs[i]);
                var maxDate = new Date(new Date(val).setDate(val.getDate() - 1));
                v.setMaxValue(maxDate);
                v.isValid();
                v.on('expand', function () {      //設定預設值 by David Huang 2014/12/01
                    PCCT_setDefValue(this);
                }, v, {
                        single: true //僅註冊一次
                    });
            }
        }
    } else {         // fix when val is null, by Chilun 2014/06/26
        try {
            if (minFields != "") {
                var mins = minFields.split(",");

                for (var i = 0; i < mins.length; i++) {
                    v = f.up('form').getForm().findField(mins[i]);
                    v.setMinValue('');
                    v.isValid();
                }
            }
            if (maxFields != '') {
                var maxs = maxFields.split(",");
                for (var i = 0; i < maxs.length; i++) {
                    v = f.up('form').getForm().findField(maxs[i]);
                    v.setMaxValue('');
                    v.isValid();
                }
            }
        } catch (e) {

        }
    }
}

//這個model是給所有combobox共用的fix by Tohow 2013/09/25
Ext.define('pComboModel', {
    extend: 'Ext.data.Model',
    fields: ['VAL', 'DISP']
});

Ext.util.Format.comboStoreRenderer = function (store) {
    return function (value) {
        var va = [];
        //var vv = value;
        //if (!Ext.isArray(value)) {
        //    vv = value.split(",");
        //}

        //這個value當型態為number時, split不被接受而發生意外錯誤 fix by Chilun 2014/04/07
        //--------------------------------------------------------------------------------
        var vv = "";
        if (typeof (value) == "number") {
            vv = value.toString();
        } else {
            vv = value;
        }
        if (!Ext.isArray(vv)) {
            vv = vv.split(",");
        }
        //--------------------------------------------------------------------------------

        Ext.each(vv, function (v) {
            if (v) {
                //var r = store.findRecord('VAL', v);       
                //這個value當英文不分大小寫造成抓取錯誤,如A=a fix by Chilun 2014/09/08
                //findRecord( fieldName, value, [startIndex], [anyMatch], [caseSensitive], [exactMatch] )
                var r = store.findRecord('VAL', v, 0, false, true);
                var s = r.get('DISP');
                va.push(s);
            }
        }, this);
        if (va) {
            return va.join(",");
        }
        return '';
    };
}

//時間區間範圍 by Tohow 2013/10/15
Ext.define('Ext.ux.form.TimePickerField', {
    extend: 'Ext.form.field.Base',
    alias: 'widget.uxtimepicker',
    alternateClassName: 'Ext.form.field.TimePickerField',
    requires: ['Ext.form.field.Number'],

    inputType: 'hidden',

    style: 'padding:4px 0 0 0;margin-bottom:0px',

    value: null,

    spinnerCfg: {
        width: 45
    },

    /** Override. */
    initComponent: function () {
        var me = this;

        me.value = me.value || Ext.Date.format(new Date(), 'H:i');

        me.callParent();// called setValue

        me.spinners = [];
        var cfg = Ext.apply({}, me.spinnerCfg, {
            readOnly: me.readOnly,
            disabled: me.disabled,
            style: 'float: left',
            listeners: {
                change: {
                    fn: me.onSpinnerChange,
                    scope: me
                }
            }
        });

        me.hoursSpinner = Ext.create('Ext.form.field.Number', Ext.apply({}, cfg, {
            minValue: 0,
            maxValue: 23
        }));
        me.minutesSpinner = Ext.create('Ext.form.field.Number', Ext.apply({}, cfg, {
            minValue: 0,
            maxValue: 59
        }));

        me.spinners.push(me.hoursSpinner, me.minutesSpinner);

    },
    /**
	* @private
	* Override.
	*/
    onRender: function () {
        var me = this, spinnerWrapDom, spinnerWrap;
        me.callParent(arguments);

        spinnerWrapDom = Ext.dom.Query.select('td', this.getEl().dom)[1]; // 4.0 ->4.1 div->td
        spinnerWrap = Ext.get(spinnerWrapDom);
        me.callSpinnersFunction('render', spinnerWrap);

        Ext.core.DomHelper.append(spinnerWrap, {
            tag: 'div',
            cls: 'x-form-clear-left'
        });

        this.setRawValue(this.value);
    },

    _valueSplit: function (v) {
        if (Ext.isDate(v)) {
            v = Ext.Date.format(v, 'H:i');
        }
        var split = v.split(':');
        return {
            h: split.length > 0 ? split[0] : 0,
            m: split.length > 1 ? split[1] : 0
        };
    },
    onSpinnerChange: function () {
        if (!this.rendered) {
            return;
        }
        this.fireEvent('change', this, this.getValue(), this.getRawValue());
    },

    callSpinnersFunction: function (funName, args) {
        for (var i = 0; i < this.spinners.length; i++) {
            this.spinners[i][funName](args);
        }
    },
    // @private get time as object,
    getRawValue: function () {
        if (!this.rendered) {
            var date = this.value || new Date();
            return this._valueSplit(date);
        } else {
            return {
                h: this.hoursSpinner.getValue(),
                m: this.minutesSpinner.getValue()
            };
        }
    },

    // private
    setRawValue: function (value) {
        value = this._valueSplit(value);
        if (this.hoursSpinner) {
            this.hoursSpinner.setValue(value.h);
            this.minutesSpinner.setValue(value.m);
        }
    },
    // overwrite
    getValue: function () {
        var v = this.getRawValue();
        return Ext.String.leftPad(v.h, 2, '0') + ':' + Ext.String.leftPad(v.m, 2, '0');
    },
    // overwrite
    setValue: function (value) {
        this.value = Ext.isDate(value) ? Ext.Date.format(value, 'H:i') : value;
        if (!this.rendered) {
            return;
        }
        this.setRawValue(this.value);
        this.validate();
    },
    // overwrite
    disable: function () {
        this.callParent(arguments);
        this.callSpinnersFunction('disable', arguments);
    },
    // overwrite
    enable: function () {
        this.callParent(arguments);
        this.callSpinnersFunction('enable', arguments);
    },
    // overwrite
    setReadOnly: function () {
        this.callParent(arguments);
        this.callSpinnersFunction('setReadOnly', arguments);
    },
    // overwrite
    clearInvalid: function () {
        this.callParent(arguments);
        this.callSpinnersFunction('clearInvalid', arguments);
    },
    // overwrite
    isValid: function (preventMark) {
        return this.hoursSpinner.isValid(preventMark) && this.minutesSpinner.isValid(preventMark);
    },
    // overwrite
    validate: function () {
        return this.hoursSpinner.validate() && this.minutesSpinner.validate();
    }
});

//時間區間範圍 by Tohow 2013/10/15, by Chilun Ho 2014/06/04
Ext.define('Ext.ux.DateTimePicker', {
    extend: 'Ext.picker.Date',
    alias: 'widget.uxdatetimepicker',
    todayText: '確認',    //原"現在",改成"確認"  by Chilun Ho 2014/06/04
    timeLabel: '時間',
    minT: '',
    maxT: '',

    initComponent: function () {
        // keep time part for value
        var value = this.value || new Date();
        this.callParent();
        this.value = value;
    },
    onRender: function (container, position) {
        if (!this.timefield) {
            this.timefield = Ext.create('Ext.ux.form.TimePickerField', {
                fieldLabel: this.timeLabel,
                labelWidth: 40,
                value: Ext.Date.format(this.value, 'H:i')
            });
        }
        this.timefield.ownerCt = this;
        this.timefield.on('change', this.timeChange, this);
        this.callParent(arguments);

        var table = Ext.get(Ext.DomQuery.selectNode('div:last', this.el.dom));
        var tfEl = Ext.core.DomHelper.insertBefore(table, {
            tag: 'div',
            style: 'border:0px;',
            children: [{
                tag: 'div',
                cls: 'x-datepicker-footer ux-timefield'
            }]
        }, true);
        this.timefield.render(this.el.child('div div.ux-timefield'));

        var p = this.getEl().parent('div.x-layer');
        if (p) {
            p.setStyle("height", p.getHeight() + 31);
        }
    },
    // listener ??域修改, timefield change
    timeChange: function (tf, time, rawtime) {
        this.value = this.fillDateTime(this.value);
    },
    // @private
    fillDateTime: function (value) {
        if (this.timefield) {
            var rawtime = this.timefield.getRawValue();
            value.setHours(rawtime.h);
            value.setMinutes(rawtime.m);
        }
        return value;
    },
    // @private
    changeTimeFiledValue: function (value) {
        this.timefield.un('change', this.timeChange, this);
        this.timefield.setValue(this.value);
        this.timefield.on('change', this.timeChange, this);
    },

    /* TODO ??值与?入框?定, 考?: ?建this.timeValue ?日期和??分?保存. */
    // overwrite
    setValue: function (value) {
        this.value = value;
        this.changeTimeFiledValue(value);
        return this.update(this.value);
    },

    getValue: function () {
        return this.fillDateTime(this.value);
    },

    // overwrite : fill time before setValue
    handleDateClick: function (e, t) {
        var me = this, handler = me.handler;

        e.stopEvent();
        if (!me.disabled && t.dateValue && !Ext.fly(t.parentNode).hasCls(me.disabledCellCls)) {
            me.doCancelFocus = me.focusOnSelect === false;
            me.setValue(this.fillDateTime(new Date(t.dateValue))); // overwrite: fill time before setValue
            delete me.doCancelFocus;
            me.fireEvent('select', me, me.value);
            if (handler) {
                handler.call(me.scope || me, me, me.value);
            }
            me.onSelect();
        }
    },

    // overwrite : fill time before setValue
    selectToday: function () {
        var me = this,
            btn = me.todayBtn,
            handler = me.handler;

        if (btn && !btn.disabled) {
            // me.setValue(Ext.Date.clearTime(new Date())); //src
            // 移除該setValue為現在日期時間, 因無法設定已更改時間  Chilun Ho 2014/06/04
            //me.setValue(new Date());// overwrite: fill time before setValue
            me.fireEvent('select', me, me.value);
            if (handler) {
                handler.call(me.scope || me, me, me.value);
            }
            me.onSelect();
        }
        return me;
    }
});

//時間區間範圍 by Tohow 2013/10/15
Ext.define('Ext.ux.form.DateTimeField', {
    extend: 'Ext.form.field.Date',
    alias: 'widget.uxdatetimefield',
    maxT: '',
    minT: '',

    initComponent: function () {
        this.format = this.format + ' ' + 'H:i';
        this.callParent();
    },

    setMaxValue: function (dt) {
        var me = this,
            picker = me.picker,
            maxValue = (Ext.isString(dt) ? me.parseDate(dt) : dt);

        me.maxT = Ext.Date.format(dt, this.format);
        me.maxValue = maxValue;
        if (picker) {
            picker.maxText = Ext.String.format(me.maxText, me.formatDate(me.maxValue));
            picker.setMaxDate(maxValue);
        }
    },

    setMinValue: function (dt) {
        var me = this,
            picker = me.picker,
            minValue = (Ext.isString(dt) ? me.parseDate(dt) : dt);

        me.minT = Ext.Date.format(dt, this.format);
        me.minValue = minValue;
        if (picker) {
            picker.minText = Ext.String.format(me.minText, me.formatDate(me.minValue));
            picker.setMinDate(minValue);
        }
    },

    validateValue: function (value) {
        var me = this,
            errors = me.getErrors(value);

        for (var i = errors.length - 1; i > -1; i--) {
            if (errors[i].indexOf("日期必須在") > -1) errors.pop();
        }
        if (me.minValue) {
            var nv = new Date(me.minT);
            if (me.getValue() < nv) {
                var mss = "此欄位之日期必須在 " + Ext.Date.format(nv, this.format) + " 之後";
                errors.push(mss);
            }
        }
        else if (me.maxValue) {
            var xv = new Date(me.maxT);
            if (me.getValue() > xv) {
                var mss = "此欄位之日期必須在 " + Ext.Date.format(xv, this.format) + " 之前";
                errors.push(mss);
            }
        }

        var isValid = Ext.isEmpty(errors);

        if (!me.preventMark) {
            if (isValid) {
                me.clearInvalid();
            } else {
                me.markInvalid(errors);
            }
        }

        return isValid;
    },

    // overwrite
    createPicker: function () {
        var me = this,
            format = Ext.String.format;

        return Ext.create('Ext.ux.DateTimePicker', {
            ownerCt: me.ownerCt,
            renderTo: document.body,
            floating: true,
            hidden: true,
            focusOnShow: true,
            minDate: me.minValue,
            maxDate: me.maxValue,
            disabledDatesRE: me.disabledDatesRE,
            disabledDatesText: me.disabledDatesText,
            disabledDays: me.disabledDays,
            disabledDaysText: me.disabledDaysText,
            format: me.format,
            showToday: me.showToday,
            startDay: me.startDay,
            minText: format(me.minText, me.formatDate(me.minValue)),
            maxText: format(me.maxText, me.formatDate(me.maxValue)),
            listeners: {
                scope: me,
                select: me.onSelect
            },
            keyNavConfig: {
                esc: function () {
                    me.collapse();
                }
            }
        });
    }
});

/* 修改CheckboxGroup的bug，加入人員 612764_3410 陳思評 2013/06/04 */
Ext.define('TRA.form.CheckboxGroup', {
    override: 'Ext.form.CheckboxGroup',
    clearInvalid: function () {
        // Clear the message and fire the 'valid' event
        var me = this,
            hadError = me.hasActiveError();

        delete me.needsValidateOnEnable;
        me.unsetActiveError();
        //拿掉下面程式
        //if (hadError) {
        //    me.setError('');
        //}
    }
});

/* Form加入clear功能，加入人員 612764_3410 陳思評 2013/06/04 */
Ext.define('TRA.form.Form', {
    override: 'Ext.form.Basic',
    clear: function () {
        Ext.suspendLayouts();

        var me = this,
            fields = me.getFields().items,
            f,
            fLen = fields.length;

        for (f = 0; f < fLen; f++) {
            if (fields[f].value != '<a href="#">關聯</a>') {
                fields[f].setValue('');//fields[f].reset();
            }
            fields[f].clearInvalid();
        }

        Ext.resumeLayouts(true);

        //if (resetRecord === true) {
        delete me._record;
        //}
        return me;
    }
});

/* 此函式用於 How to put X inside textfield to clear text on Combo in extjs，加入人員 TWSM 何季倫 2013/06/05 */
Ext.define('Ext.ux.form.field.ClearCombo', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.clearcombo',
    trigger2Cls: 'x-form-clear-trigger',

    initComponent: function () {
        var me = this;
        me.addEvents(
            /**
            * @event beforeclear
            *
            * @param {<|#NAMESPACE#|>.FilterCombo} FilterCombo The filtercombo that triggered the event
            */
            'beforeclear',
            /**
            * @event beforeclear
            *
            * @param {<|#NAMESPACE#|>.FilterCombo} FilterCombo The filtercombo that triggered the event
            */
            'clear'
        );

        me.callParent(arguments);
        me.on('specialkey', this.onSpecialKeyDown, me);
        me.on('select', function (me, rec) {
            me.onShowClearTrigger(true);
        }, me);
        me.on('afterrender', function () { me.onShowClearTrigger(false); }, me);
    },

    /**
    * @private onSpecialKeyDown
    * eventhandler for special keys
    */
    onSpecialKeyDown: function (obj, e, opt) {
        if (e.getKey() == e.ESC) {
            this.clear();
            this.setValue('');
        }
    },

    onShowClearTrigger: function (show) {
        var me = this;

        show = (Ext.isBoolean(show)) ? show : false;
        if (show) {
            me.triggerEl.each(function (el, c, i) {
                if (i === 1) {
                    el.setWidth(el.originWidth, false);
                    el.setVisible(true);
                    me.active = true;
                }
            });
        } else {
            me.triggerEl.each(function (el, c, i) {
                if (i === 1) {
                    el.originWidth = el.getWidth();
                    el.setWidth(0, false);
                    el.setVisible(false);
                    me.active = false;
                }
            });
        }
        // ToDo -> Version specific methods
        if (Ext.lastRegisteredVersion.shortVersion > 407) {
            me.updateLayout();
        } else {
            me.updateEditState();
        }
    },

    /**
    * @override onTrigger2Click
    * eventhandler
    */
    onTrigger2Click: function (args) {
        this.clear();
        this.setValue('');
    },

    /**
    * @private clear
    * clears the current search
    */
    clear: function () {
        var me = this;
        me.fireEvent('beforeclear', me);
        me.clearValue();
        me.onShowClearTrigger(false);
        me.fireEvent('clear', me);
    }
});

/* 設定 MMIS library 參考路徑 加入人員 614346_3410 李宗霖 2014/05/27 */
Ext.Loader.setPath('MMIS', '../../../Scripts/app');
Ext.Loader.setPath('MMIS.DS', '../../../Scripts/DS');
Ext.Loader.setPath('MMIS.TP.view', '../../../Scripts/TP');
Ext.Loader.setPath('Ext.ux', '../../../Scripts/extjs/ux');

/* 設定 MMIS library 參考路徑 加入人員 612764_3410 陳思評 2014/06/03 */
Ext.Loader.setPath('MMIS.TP', '../../../Scripts/TP');

/* 刷新簽核區(由ifram內刷新)，加入人員 612764_3410 陳思評 2014/05/27 */
function callParDeltaFlow() {

    var d = new Date();
    var month = d.getMonth() + 1;
    var day = d.getDate();
    var hour = d.getHours();
    var minutes = d.getMinutes();
    var seconds = d.getSeconds();
    var NowDate = d.getFullYear() + '-' + toTen(month) + '-' + toTen(day);
    var Now = toTen(hour) + ":" + toTen(minutes) + ":" + toTen(seconds);
    //var Now = d.getFullYear() + '-' + toTen(month) + '-' + toTen(day) + ' ' + toTen(hour) + ":" + toTen(minutes)+":" +toTen(seconds);

    //var flowCount = "";
    //var flowCount2 = "";
    //Ext.Ajax.request({
    //    url: '../../../api/GetTTLCount/CalTTL',
    //    method: 'POST',
    //    success: function (response) {
    //        responseText = Ext.decode(response.responseText);
    //        if (responseText.success) {
    //            flowCount = responseText.ds.DeltaFlow[0].flowcount1;
    //            flowCount2 = responseText.ds.DeltaFlow[0].flowcount2;
    //        } else {
    //            flowCount = "簽核失敗!";
    //        }
    //    },
    //    failure: function (response, options) {
    //        flowCount = "簽核失敗!";
    //    },
    //    callback: function () {
    //        if (flowCount == "0") {
    //            $('#flow', top.document)[0].innerHTML = "待簽核:<span style='color:#FF0000'>" + flowCount + "筆</span>";
    //        }
    //        if (flowCount2 == "0") {
    //            $('#flow2', top.document)[0].innerHTML = "|代理人待簽核:<span style='color:#FF0000'>" + flowCount2 + "筆</span>&nbsp;<span style='color:#000000;font-style: italic;'>最後更新時間" + Now + "</span>";
    //        }
    //        if (flowCount != "0" && flowCount != "簽核失敗!") {
    //            $('#flow', top.document)[0].innerHTML = "待簽核:<span style='color:#FF0000'>" + flowCount + "筆</span><img src='../Images/TRA/new.gif' alt='new'>";
    //        }
    //        if (flowCount2 != "0" && flowCount != "簽核失敗!") {
    //            $('#flow2', top.document)[0].innerHTML = "|代理人待簽核:<span style='color:#FF0000'>" + flowCount2 + "筆</span><img src='../Images/TRA/new.gif' alt='new'>&nbsp;<span style='color:#000000;font-style: italic;'>最後更新時間" + Now + "</span>";
    //        }
    //        if (flowCount == "簽核失敗!") {
    //            $('#flow', top.document)[0].innerHTML = "待簽核:<span style='color:#FF0000'>" + "</span>";
    //            $('#flow2', top.document)[0].innerHTML = "";
    //        }
    //    }
    //});
}
function toTen(s) {
    return s < 10 ? '0' + s : s;
}
/* 數字千分位，加入人員 612764_3410 陳思評 2014/05/29 */
var numberToCurrency = function (n) {
    Ext.util.Format.thousandSeparator = ',';
    Ext.util.Format.decimalSeparator = '.';
    return Ext.util.Format.number(n, '0,000');
    //return Ext.util.Format.currency(n,'￥',2,false);
}

/* TRA要求將刪除提示對話窗預設按鈕為否，加入人員 612764_3410 陳思評,Chilun Ho 2014/06/04 */
//Ext.Msg.defaultButton = 'no';

/* 將數字欄位顯示成有千分位符號，加入人員 Tohow,Chilun 2014/06/17 */
/* 解決會減少輸入值之最大值，是故須覆寫(OVERWRIT)輸入值值之最大值及長度值，加入人員 Chilun 2014/10/09 */
Ext.define('Ext.ux.form.NumericField',
    {
        extend: 'Ext.form.field.Number',//Extending the NumberField
        alias: 'widget.numericfield',//Defining the xtype,

        useThousandSeparator: false,

        initValue: function () {
            var me = this

            if (!isNaN(me.maxLength)) {
                me.maxLength = me.maxLength + (Math.ceil(me.maxLength / 3) - 1);
            }
        },
        /**
    * @inheritdoc
    */
        toRawNumber: function (value) {
            return String(value).replace(this.decimalSeparator, '.').replace(new RegExp(Ext.util.Format.thousandSeparator, "g"), '');
        },

        /**
    * @inheritdoc
    */
        getErrors: function (value) {
            if (!this.useThousandSeparator)
                return this.callParent(arguments);
            var me = this,
                errors = Ext.form.field.Text.prototype.getErrors.apply(me, arguments),
                format = Ext.String.format,
                num;

            value = Ext.isDefined(value) ? value : this.processRawValue(this.getRawValue());

            if (value.length < 1) { // if it's blank and textfield didn't flag it then it's valid
                return errors;
            }

            value = me.toRawNumber(value);

            if (isNaN(value.replace(Ext.util.Format.thousandSeparator, ''))) {
                errors.push(format(me.nanText, value));
            }

            num = me.parseValue(value);

            if (me.minValue === 0 && num < 0) {
                errors.push(this.negativeText);
            }
            else if (num < me.minValue) {
                errors.push(format(me.minText, me.valueToRaw(me.minValue)));
            }

            if (num > me.maxValue) {
                errors.push(format(me.maxText, me.valueToRaw(me.maxValue)));
            }

            return errors;
        },
        validateValue: function (value) {
            if (!Ext.form.NumberField.superclass.validateValue.call(this, value)) {
                return false;
            }
            if (value.length < 1) { // if it's blank and textfield didn't flag it then it's valid
                return true;
            }
            try {
                var num = this.parseValue(value);
                if (isNaN(num)) {
                    this.markInvalid(String.format(this.nanText, value));
                    return false;
                }
                //if (num < this.minValue) {
                //    this.markInvalid(String.format(this.minText, this.minValue));
                //    return false;
                //}
                //if (num > this.maxValue) {
                //    this.markInvalid(String.format(this.maxText, this.maxValue));
                //    return false;
                //}

                var valLen = value.replace(/,/g, '').length;
                if (valLen > this.maxLength) {
                    this.markInvalid(String.format(this.maxLengthText, this.maxLength));
                    return false;
                }
            } catch (e) {

            }
            return true;
        },
        /**
    * @inheritdoc
    */
        valueToRaw: function (value) {

            if (!this.useThousandSeparator)
                return this.callParent(arguments);
            var me = this;

            var format = "000,000";
            for (var i = 0; i < me.decimalPrecision; i++) {
                if (i == 0)
                    format += ".";
                format += "0";
            }

            //修正數值在小數會進位的問題
            var v = '' + value;
            var pos = v.indexOf(".");
            if (pos > 0) {
                var s = v.substring(pos + 1);
                if (s.length > this.decimalPrecision) {
                    var f = Math.pow(10, this.decimalPrecision);
                    value = Math.floor(v * f) / f;
                }
            }
            // end
            value = me.parseValue(Ext.util.Format.number(value, format));
            //value = me.fixPrecision(value);       // 此行註解, 因為讓小數進位
            value = Ext.isNumber(value) ? value : parseFloat(me.toRawNumber(value));
            value = isNaN(value) ? '' : String(Ext.util.Format.number(value, format)).replace('.', me.decimalSeparator);

            return value;
        },

        /**
    * @inheritdoc
    */
        getSubmitValue: function () {
            if (!this.useThousandSeparator)
                return this.callParent(arguments);
            var me = this,
                value = me.callParent();

            if (!me.submitLocaleSeparator) {
                value = me.toRawNumber(value);
            }
            value = value.replace(/,/g, '');
            return value;
        },

        /**
    * @inheritdoc
    */
        setMinValue: function (value) {
            if (!this.useThousandSeparator)
                return this.callParent(arguments);
            var me = this,
                allowed;

            me.minValue = Ext.Number.from(value, Number.NEGATIVE_INFINITY);
            me.toggleSpinners();

            // Build regexes for masking and stripping based on the configured options
            if (me.disableKeyFilter !== true) {
                allowed = me.baseChars + '';

                if (me.allowExponential) {
                    allowed += me.decimalSeparator + 'e+-';
                }
                else {
                    allowed += Ext.util.Format.thousandSeparator;
                    if (me.allowDecimals) {
                        allowed += me.decimalSeparator;
                    }
                    if (me.minValue < 0) {
                        allowed += '-';
                    }
                }

                allowed = Ext.String.escapeRegex(allowed);
                me.maskRe = new RegExp('[' + allowed + ']');
                if (me.autoStripChars) {
                    me.stripCharsRe = new RegExp('[^' + allowed + ']', 'gi');
                }
            }
        },

        /**
    * @private
    */
        parseValue: function (value) {
            if (!this.useThousandSeparator)
                return this.callParent(arguments);
            value = parseFloat(this.toRawNumber(value));
            return isNaN(value) ? null : value;
        }
    });

//志鳴新增 年月選擇 2014/7/30
Ext.define('Ext.ux.form.MonthField', {
    extend: 'Ext.form.field.Picker',
    alias: 'widget.monthfield',
    //requires: ['Ext.picker.Date'],
    //alternateClassName: ['Ext.form.DateField', 'Ext.form.Date'],
    altFormats: "m/y|m/Y|m-y|m-Y|my|mY|y/m|Y/m|y-m|Y-m|ym|Ym",
    invalidText: '',
    format: 'Xm',
    maskRe: /[0-9]/,
    regex: /^(([1-9][0-9][0-9])|([1-9][0-9]))(0[123456789]|10|11|12)$/,
    regexText: '不是正確的日期格式 - 必須像是 「10801」 這樣的格式',
    format: 'Xm',
    enforceMaxLength: true,
    maxLength: 5,
    //allowOnlyWhitespace: false,
    //submitFormat: 'Y/m/d',
    //disabledDaysText: "Disabled",
    //disabledDatesText: "Disabled",
    //minText: "The date in this field must be equal to or after {0}",
    //maxText: "The date in this field must be equal to or before {0}",
    //invalidText: "{0} is not a valid date - it must be in the format {1}",
    triggerCls: Ext.baseCSSPrefix + 'form-date-trigger',
    //showToday: true,
    //initTime: '12',
    //initTimeFormat: 'H',

    matchFieldWidth: false,

    startDay: new Date(),

    initComponent: function () {
        var me = this;
        me.disabledDatesRE = null;
        me.callParent();
    },

    initValue: function () {
        var me = this,
            value = me.value;

        if (Ext.isString(value)) {
            me.value = Ext.Date.parse(value, this.format);
        }
        if (me.value)
            me.startDay = me.value;
        me.callParent();
    },

    rawToValue: function (rawValue) {
        return Ext.Date.parse(rawValue, this.format) || rawValue || null;
    },

    valueToRaw: function (value) {
        return this.formatDate(value);
    },



    formatDate: function (date) {
        return Ext.isDate(date) ? Ext.Date.dateFormat(date, this.format) : date;
    },
    createPicker: function () {
        var me = this,
            format = Ext.String.format;

        return Ext.create('Ext.picker.Month', {
            //renderTo: me.el,
            pickerField: me,
            ownerCt: me.ownerCt,
            renderTo: document.body,
            floating: true,
            shadow: false,
            focusOnShow: true,
            listeners: {
                scope: me,
                cancelclick: me.onCancelClick,
                okclick: me.onOkClick,
                yeardblclick: me.onOkClick,
                monthdblclick: me.onOkClick
            }
        });
    },

    onExpand: function () {
        //this.picker.show();
        this.picker.setValue(this.startDay);
        //

    },

    //    onCollapse: function () {
    //        this.focus(false, 60);
    //    },

    onOkClick: function (picker, value) {
        var me = this,
            month = value[0],
            year = value[1],
            date = new Date(year, month, 1);
        me.startDay = date;
        me.setValue(date);
        this.picker.hide();
        //this.blur();
    },

    onCancelClick: function () {
        this.picker.hide();
        //this.blur();
    }

});

// 若為假日則不可選擇
Ext.define('Ext.ux.form.DateFilterField', {
    extend: 'Ext.form.field.Date',
    alias: 'widget.dateFilterField',
    disabledDatesText: "Can not be a holiday.",
    format: 'Y/m/d',
    enforceMaxLength: true, //最多輸入的長度有限制
    maxLength: 10,          //最多輸入10碼
    maskRe: /[0-9\/]/,      //前端只能輸入數字跟斜線
    regexText: '正確格式應為「YYYY/MM/DD」，<br>範圍應在「1900/01/01~2999/12/31」',
    regex: /^((((19[0-9]{2})|(2[0-9]{3}))(\/)(0[13578]|10|12)(\/)(0[1-9]|[12][0-9]|3[01]))|(((19[0-9][0-9])|(2[0-9][0-9][0-9]))(\/)(0[469]|11)(\/)([0][1-9]|[12][0-9]|30))|(((19[0-9][0-9])|(2[0-9][0-9][0-9]))(\/)(02)(\/)(0[1-9]|1[0-9]|2[0-8]))|(([02468][048]00)(\/)(02)(\/)(29))|(([13579][26]00)(\/)(02)(\/)(29))|(([0-9][0-9][0][48])(\/)(02)(\/)(29))|(([0-9][0-9][2468][048])(\/)(02)(\/)(29))|(([0-9][0-9][13579][26])(\/)(02)(\/)(29)))$/,
    initValue: function () {
        //var me = this;
        //var disabledDates = [];
        //Ext.Ajax.request({
        //    url: '../../../api/DateFilter/Query',
        //    method: 'POST',
        //    params: { format: '111' }, // format=111: yyyy/mm/dd, 112:yyyymmdd
        //    success: function (response) {
        //        var data = Ext.decode(response.responseText);
        //        if (data.success) {
        //            var tb_disableDate = data.etts;
        //            if (tb_disableDate.length > 0) {
        //                for (var i = 0; i < tb_disableDate.length; i++) {
        //                    disabledDates.push(tb_disableDate[i]);
        //                }
        //                me.setDisabledDates(disabledDates);

        //                // setDisabledDates會清除原有disable的日期,故最大值和最小值需在這裡才設定
        //                // 由於request需要時間,如在實際使用時有自訂minValue和maxValue,可考慮用setTimeout延遲一些時間後設定
        //                var getMinDate = new Date();
        //                var getMaxDate = new Date();
        //                getMinDate.setDate(getMinDate.getDate() - 365);
        //                getMaxDate.setDate(getMaxDate.getDate() + 365);
        //                me.setMinValue(getMinDate);
        //                me.setMaxValue(getMaxDate);
        //            }
        //        }
        //    },
        //    failure: function (response, options) {

        //    }
        //});
    }
});

/* 將display field顯示數字為千分位符號,顯示日期為日期格式，加入人員 Chilun 2014/09/05 */
// dateFormat, numberFormat
Ext.override(Ext.form.DisplayField, {
    getValue: function () {
        return this.value;
    },
    setValue: function (v) {
        this.value = v;
        this.setRawValue(this.formatValue(v));
        return this;
    },
    formatValue: function (v) {
        if (this.dateFormat && Ext.isDate(v)) {
            return v.dateFormat(this.dateFormat);
        }
        if (this.numberFormat && typeof v == 'number') {
            return Ext.util.Format.number(v, this.numberFormat);
        }
        return v;
    }
});

// RowNumberer顯示總筆數只以當頁為基準(預設為以全部資料為基準)
Ext.override(Ext.grid.column.RowNumberer, {
    renderer: function (value, metaData, record, rowIdx, colIdx, store) {
        var rowspan = this.rowspan;
        if (rowspan) {
            metaData.tdAttr = 'rowspan="' + rowspan + '"';
        }
        metaData.tdCls = Ext.baseCSSPrefix + 'grid-cell-special';

        return store.indexOf(record) ? (store.indexOf(record) + 1) : (rowIdx + 1);
    },
    defaultRenderer: function (value, metaData, record, rowIdx, colIdx, store) {

        return value;
    }
});

/* 清空Grid,Tool,Form恢復初設，加入人員 Chilun 2014/12/02 */
function cleanComponent(grid, tool, form) {
    grid.getStore().removeAll();
    grid.getStore().sync();
    Ext.each(tool.items.items, function (btn) {
        if (btn.getXType() == 'button') {
            btn.setDisabled(true);
        }
    });
    form.getForm().reset();
}

/* textfield+button 挑選資料，加入人員 612764_3410 陳思評 2014/11/28 */
Ext.define('Ext.form.pick.Data', {
    extend: 'Ext.form.field.Text',
    alias: ['widget.mmispickdata'],
    requires: [
        'Ext.button.Button'
    ],
    buttonText: '...',
    buttonOnly: false,
    buttonMargin: 3,
    onRender: function () {
        var me = this,
            id = me.id,
            inputEl;

        me.callParent(arguments);
        inputEl = me.inputEl;
        inputEl.dom.name = '';
        me.button = new Ext.button.Button(Ext.apply({
            renderTo: id + '-browseButtonWrap',
            id: id + '-button',
            ui: me.ui,
            disabled: me.disabled,
            text: me.buttonText,
            style: me.buttonOnly ? '' : 'margin-left:' + me.buttonMargin + 'px',
            inputName: me.getName(),
            listeners: {
                scope: me,
                click: function () {
                    var pwidth = $(window).innerWidth();
                    var pheight = $(window).innerHeight();
                    var viewport = {
                        width: pwidth,
                        height: pheight
                    }
                    showPopForm(me.param1, me.param2, me.param3, viewport);
                }
            }
        }, me.buttonConfig));
    },

    getSubTplMarkup: function (values) {
        var me = this,
            field = me.callParent(arguments);
        return '<table style="width:' + me.pickerWidth + 'px" id="' + me.id + '-triggerWrap" cellpadding="0" cellspacing="0"><tbody><tr>' +
            '<td id="' + me.id + '-inputCell">' + field + '</td>' +
            '<td id="' + me.id + '-browseButtonWrap"></td>' +
            '</tr></tbody></table>';
    }
});

/* PagingToolbar加入動態調整筆數功能，加入人員 612764_3410 陳思評 2015/12/24 */
Ext.define('Ext.ux.grid.PageSize', {
    //extend: 'Ext.form.field.Combobox',
    extend: 'Ext.form.field.Text',
    alias: 'plugin.pagesize',
    beforeText: '顯示1頁',
    afterText: '筆',
    //mode: 'local',
    //displayField: 'text',
    //valueField: 'value',
    allowBlank: false,
    triggerAction: 'all',
    width: 50,
    maskRe: /[0-9]/,
    /**
    * initialize the paging combo after the pagebar is randered
    */
    init: function (paging) {
        //this.store = Ext.create('Ext.data.ArrayStore', {
        //    fields: ['text', 'value'],
        //    data: [['10', 10], ['20', 20], ['30', 30], ['40', 40], ['50', 50]]
        //});
        if (this.pageSize) {
            paging.store.pageSize = this.pageSize;
            this.setValue(this.pageSize);
        }
        paging.on('afterrender', this.onInitView, this);
    },
    /**
    * passing the select and specialkey events for the combobox
    * after the pagebar is rendered.
    */
    onInitView: function (paging) {
        this.setValue(paging.store.pageSize);
        paging.add('-', this.beforeText, this, this.afterText);
        this.on('select', this.onPageSizeChanged, paging);
        this.on('specialkey', function (combo, e) {
            if (13 === e.getKey()) {
                this.onPageSizeChanged.call(paging, this);
            }
        });
    },

    /**
    * refresh the page when the value is changed
    */
    onPageSizeChanged: function (combo) {
        this.store.pageSize = parseInt(combo.getRawValue(), 10);
        this.moveFirst();
    }
});

Ext.define('Ext.toolbar.dynamic.Paging', {
    extend: 'Ext.toolbar.Paging',
    alias: 'widget.mmispagingtoolbar',
    pageCount: '筆',

    pageSize: 20,//default

    initComponent: function () {
        var me = this;
        me.store.on('beforeload', function (store, operation) {
            operation.limit = me.down("#inputItem2").value;
        })
        me.callParent(arguments);

        me.down("#inputItem2").setValue(me.pageSize);

    },

    getPagingItems: function () {
        var me = this;

        return [{
            itemId: 'first',
            tooltip: me.firstText,
            overflowText: me.firstText,
            iconCls: Ext.baseCSSPrefix + 'tbar-page-first',
            disabled: true,
            handler: me.moveFirst,
            scope: me
        }, {
            itemId: 'prev',
            tooltip: me.prevText,
            overflowText: me.prevText,
            iconCls: Ext.baseCSSPrefix + 'tbar-page-prev',
            disabled: true,
            handler: me.movePrevious,
            scope: me
        },
            '-',
        me.beforePageText,
        {
            xtype: 'numberfield',
            itemId: 'inputItem',
            name: 'inputItem',
            cls: Ext.baseCSSPrefix + 'tbar-page-number',
            allowDecimals: false,
            minValue: 1,
            hideTrigger: true,
            enableKeyEvents: true,
            keyNavEnabled: false,
            selectOnFocus: true,
            submitValue: false,

            isFormField: false,
            width: me.inputItemWidth,
            margins: '-1 2 3 2',
            listeners: {
                scope: me,
                keydown: me.onPagingKeyDown,
                blur: me.onPagingBlur
            }
        }, {
            xtype: 'tbtext',
            itemId: 'afterTextItem',
            text: Ext.String.format(me.afterPageText, 1)
        },
            '-',
        {
            itemId: 'next',
            tooltip: me.nextText,
            overflowText: me.nextText,
            iconCls: Ext.baseCSSPrefix + 'tbar-page-next',
            disabled: true,
            handler: me.moveNext,
            scope: me
        }, {
            itemId: 'last',
            tooltip: me.lastText,
            overflowText: me.lastText,
            iconCls: Ext.baseCSSPrefix + 'tbar-page-last',
            disabled: true,
            handler: me.moveLast,
            scope: me
        },
            '-',
        {
            itemId: 'refresh',
            tooltip: me.refreshText,
            overflowText: me.refreshText,
            iconCls: Ext.baseCSSPrefix + 'tbar-loading',
            handler: me.doRefresh,
            scope: me
        }, '-', {
            xtype: 'numberfield',
            itemId: 'inputItem2',
            name: 'inputItem2',
            cls: Ext.baseCSSPrefix + 'tbar-page-number',
            allowDecimals: false,
            minValue: 1,
            hideTrigger: true,
            enableKeyEvents: true,
            keyNavEnabled: false,
            selectOnFocus: true,
            submitValue: false,
            isFormField: false,
            width: me.inputItemWidth,
            margins: '-1 2 3 2',
            listeners: {
                scope: me,
                keydown: me.onPagingKeyDown,
                blur: me.onPagingBlur
            }
        },
        me.pageCount];
    }

});


/* Gird複製文字功能，加入人員 612764_3410 陳思評 2015/03/19 */
Ext.define('TRA.grid.View', {
    override: 'Ext.grid.View',
    enableTextSelection: true
});

//設置fieldstyle,唯讀/必填 傳入form(form.getForm()),FieldName(string),唯讀/唯讀清空/必填/白色啟用 readonly/readonlyC/required/normal (string) ,文字對齊靠左/靠右 left /right (string)
//ex.  styleSet(T1Form.getForm() , 'IA_RSL_DES','readonly','');        by David Huang 2015/03/24
function styleSet(form, fieldname, status, align) {
    var f = form;
    var fn = fieldname;
    var st = status;
    var al = align;
    var v;
    var x1, x2, x3;

    v = f.findField(fn);
    switch (al) {
        case 'left':
            x2 = ';text-align:left';
            break;
        case 'right':
            x2 = ';text-align:right';
            break;
        default:
            x2 = '';
            break;
    }


    switch (status) {
        case 'readonly':
            x1 = "background:lightgray";
            x3 = x1 + x2;
            v.setReadOnly(true);
            v.setFieldStyle(x3);
            v.allowBlank = true;
            v.validate();
            break;
        case 'readonlyC':
            x1 = "background:lightgray";
            x3 = x1 + x2;
            v.setReadOnly(true);
            v.setValue('');
            v.allowBlank = true;
            v.setFieldStyle(x3);
            v.validate();
            break;
        case 'required':
            x1 = "background:pink";
            x3 = x1 + x2;
            v.allowBlank = false;
            v.setReadOnly(false);
            v.setFieldStyle(x3);
            v.validate();
            break;
        case 'normal':
            x1 = "background:white";
            x3 = x1 + x2;
            v.setReadOnly(false);
            v.allowBlank = true;
            v.setFieldStyle(x3);
            v.validate();
            break;
    }

}

//定義自動轉換大寫TextField ,設Xtype:'upperCaseTextField'   by David Huang 2015/04/07
Ext.define('IN.view.item.UpperCaseTextField', {
    extend: 'Ext.form.field.Text',
    alias: 'widget.upperCaseTextField',
    fieldStyle: {
        textTransform: "uppercase"
    },
    initComponent: function () {
        this.callParent(arguments);
    },
    listeners: {
        change: function (obj, Value) {
            obj.setRawValue(Value.toUpperCase());
        }
    }
});

//類似Oracle SQL NVL功能 by David Huang 2015/04/07
function nvl(value, dvalue) {
    var v = value;
    var dv = dvalue;
    if (v == null || v == undefined) {
        return dv;
    }
    else {
        return v;
    }
}

//修正 Extjs Pre-release 版當 form 裡面包含檔案上傳欄位時，無法判斷網路連線失敗問題 by 宗霖 2015/07/17
Ext.define('MMIS.override.data.Connection', {
    override: 'Ext.data.Connection',
    onUploadComplete: function (frame, options) {
        var me = this,

            response = {
                responseText: '',
                responseXML: null
            }, callback, success, doc, contentNode;

        try {
            doc = frame.contentWindow.document || frame.contentDocument || window.frames[frame.id].document;


            if (doc) {
                if (Ext.isOpera && doc.location == 'about:blank') {
                    return;
                }
                if (doc.body) {



                    if ((contentNode = doc.body.firstChild) && /pre/i.test(contentNode.tagName)) {
                        response.responseText = contentNode.textContent;
                    }



                    else if ((contentNode = doc.getElementsByTagName('textarea')[0])) {
                        response.responseText = contentNode.value;
                    }

                    else {
                        response.responseText = doc.body.textContent || doc.body.innerText;
                    }
                }

                response.responseXML = doc.XMLDocument || doc;
                callback = options.success;
                success = true;
            }
        } catch (e) {

            response.responseText = '{success:false,message:"' + Ext.String.trim(e.message || e.description) + '"}';
            callback = options.failure;
            success = false;
        }

        me.fireEvent('requestcomplete', me, response, options);

        Ext.callback(callback, options.scope, [response, options]);
        Ext.callback(options.callback, options.scope, [options, success, response]);

        setTimeout(function () {
            Ext.removeNode(frame);
        }, 100);
    }
});

var GetPopWin = function (viewport, popform, strTitle, winActWidth, winActHeight) {
    var win = Ext.widget('window', {
        title: strTitle,
        id: 'win',
        modal: true,
        layout: 'fit',
        autoScroll: true,
        closeAction: 'destroy',
        constrain: true,
        resizable: true,
        closable: false,
        width: winActWidth,
        height: winActHeight,
        items: popform,
        listeners: {
            move: function (xwin, x, y, eOpts) {
                xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                //xwin.suspendEvent('move');
                //xwin.center();
                //xwin.resumeEvent('move');
            },
            resize: function (xwin, width, height) {
                winActWidth = width;
                winActHeight = height;
            }
        }
    });
    return win;
};

Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}
Date.prototype.addMonth = function (month) {
    var date = new Date(this.valueOf());
    date.setMonth(date.getMonth() + month);
    return date;
}

// 取得民國年今日日期,如1080101
function getChtToday() {
    var year = new Date().getFullYear();
    var month = (new Date().addMonth(1).getMonth()) > 9 ? (new Date().addMonth(1).getMonth()).toString() : "0" + (new Date().addMonth(1).getMonth()).toString();
    var date = (new Date().getDate()) > 9 ? new Date().getDate().toString() : "0" + new Date().getDate().toString();

    return (year - 1911).toString() + month.toString() + date.toString();
}

// 取得民國年日期(增減日)
function getShiftDaysChtDate(shiftDays) {
    var year = new Date().addDays(shiftDays).getFullYear();
    var month = (new Date().addDays(shiftDays).getMonth() + 1) > 9 ? (new Date().addDays(shiftDays).getMonth() + 1).toString() : "0" + (new Date().addDays(shiftDays).getMonth() + 1).toString();
    var date = (new Date().addDays(shiftDays).getDate()) > 9 ? new Date().addDays(shiftDays).getDate().toString() : "0" + new Date().addDays(shiftDays).getDate().toString();

    return (year - 1911).toString() + month.toString() + date.toString();
}

// 取得民國年日期(增減月)
function getShiftMonthChtDate(shiftMonth) {
    var year = new Date().addMonth(shiftMonth).getFullYear();
    var month = (new Date().addMonth(1 + shiftMonth).getMonth()) > 9 ? new Date().addMonth(1 + shiftMonth).getMonth().toString() : "0" + new Date().addMonth(1 + shiftMonth).getMonth().toString();
    var date = (new Date().getDate()) > 9 ? new Date().getDate().toString() : "0" + new Date().getDate().toString();

    return (year - 1911).toString() + month.toString() + date.toString();
}

// 查詢院內碼並選取後回傳院內碼 - 湘倫
popMmcodeForm = function (viewport, url, p, closeCallback) {
    var viewModel = Ext.create('WEBAPP.store.MI_MAST_ViewModel');
    var MmcodeStore = viewModel.getStore('MMCODE');
    var wh_no = (p != null) ? ((p.WH_NO == null || p.WH_NO == undefined) ? '' : p.WH_NO)
        : '';
    var docno = (p != null) ? ((p.DOCNO == null || p.DOCNO == undefined) ? '' : p.DOCNO)
        : '';
    function MmcodeLoad(mmcode, mmcode_c, mmcode_e, matclass) {
       
        if (wh_no != null || wh_no != '') {
            MmcodeStore.getProxy().setExtraParam("WH_NO", wh_no);
        }
        if (docno != null || docno != '') {
            MmcodeStore.getProxy().setExtraParam("DOCNO", docno);
        }

        //MmcodeStore.getProxy().setExtraParam("IS_INV", p.IS_INV);
        //MmcodeStore.getProxy().setExtraParam("E_IFPUBLIC", p.E_IFPUBLIC); // 是否公藥  0-非公藥, 1-存點為病房，上級庫為住院藥局 PH1A, 2-存點為病房，上級庫為藥庫 PH1S, 3-存點為病房，設為備用藥，上級庫為住院藥局

        //Ext.apply(MmcodeStore.getProxy().extraParams, p);   // 由外部參數傳入
        MmcodeStore.getProxy().setExtraParam("MMCODE", mmcode);
        MmcodeStore.getProxy().setExtraParam("MMNAME_C", mmcode_c);
        MmcodeStore.getProxy().setExtraParam("MMNAME_E", mmcode_e);
         MmcodeStore.getProxy().setExtraParam("MAT_CLASS", matclass);
        MmcodeStore.getProxy().setUrl(url);
        MmcodeTool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var MmcodeQuery = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            //padding: '4 0 4 0'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    name: 'MMCODE1',
                    fieldLabel: '院內碼',
                    labelAlign: 'right',
                    enforceMaxLength: true,
                    maxLength: 13,
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_C',
                    fieldLabel: '中文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_E',
                    fieldLabel: '英文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'button',
                    id: 'btnSearch',
                    text: '查詢',
                    handler: function () {
                        var f = MmcodeQuery.getForm();
                        MmcodeLoad(f.findField('MMCODE1').getValue(), f.findField('MMNAME_C').getValue(), f.findField('MMNAME_E').getValue(), p.MAT_CLASS);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('MMCODE1').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var MmcodeTool = Ext.create('Ext.PagingToolbar', {
        store: MmcodeStore,
        displayInfo: true,
        border: false,
        plain: true
    });

    var MmcodeGrid = Ext.create('Ext.grid.Panel', {
        store: MmcodeStore,
        autoScroll: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [MmcodeQuery]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [MmcodeTool]
            }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'SINGLE'
        //},
        //selType: 'checkboxmodel',
        //viewConfig: {
        //    listeners: {
        //        // 讓column寬度可以隨著內容自動縮放
        //        refresh: function (dataview) {
        //            Ext.each(dataview.panel.columns, function (column) {
        //                if (column.autoSizeColumn === true)
        //                    column.autoSize();
        //            })
        //        }
        //    }
        //},
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                align: 'left',
                //style: 'text-align:center',
                width: 70
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                align: 'left',
                //style: 'text-align:center',
                //autoSizeColumn: true
                width: 300
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                align: 'left',
                //style: 'text-align:center',
                //autoSizeColumn: true
                width: 300
            },
            {
                text: "進貨單價",
                dataIndex: 'M_CONTPRICE',
                align: 'left',
                width: 100
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                align: 'left',
                width: 100
            },
            {
                text: "庫房名稱",
                dataIndex: 'AGEN_NAMEC',
                hidden: true
            },
            {
                text: "進貨單價",
                dataIndex: 'M_DISCPERC',
                hidden: true
            },
            {
                text: "計費方式",
                dataIndex: 'M_PAYID',
                hidden: true
            },
            {
                text: "庫備識別碼",
                dataIndex: 'M_STOREID',
                hidden: true
            },
            {
                text: "合約識別碼",
                dataIndex: 'M_CONTID',
                hidden: true
            },
            {
                text: "申請申購識別碼",
                dataIndex: 'M_APPLYID',
                hidden: true
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            //height: '100%',
            //layout: 'fit',
            width: 300,
            height: 300,
            closable: false,
            autoScroll: true,
            items: [MmcodeGrid],
            buttons: [
                {
                    //id: 'winclosed',
                    disabled: false,
                    text: '選取',
                    handler: function () {
                        var selection = MmcodeGrid.getSelection();
                        if (selection.length) {
                            let mmcode = '', mmname_c = '', mmname_e = '', m_contprice = '', base_unit = '', m_discperc = '', agen_namec = '';
                            // selection.map這段在IE不支援,三總初期雖說主要會使用CHROME,
                            // 但後續仍有可能使用舊型PDA是搭載IE,且壓力測試腳本之前也是在IE環境進行,
                            // 改為使用$.map
                            //selection.map(item => {
                            //    mmcode = item.get('MMCODE');
                            //    mmname_c = item.get('MMNAME_C');
                            //    mmname_e = item.get('MMNAME_E');
                            //    m_contprice = item.get('M_CONTPRICE');
                            //    base_unit = item.get('BASE_UNIT');
                            //    m_discperc = item.get('M_DISCPERC');
                            //    agen_namec = item.get('AGEN_NAMEC');
                            //});
                            $.map(selection, function (item, key) {
                                mmcode = item.get('MMCODE');
                                mmname_c = item.get('MMNAME_C');
                                mmname_e = item.get('MMNAME_E');
                                m_contprice = item.get('M_CONTPRICE');
                                base_unit = item.get('BASE_UNIT');
                                m_discperc = item.get('M_DISCPERC');
                                agen_namec = item.get('AGEN_NAMEC');
                            })

                            this.up('window').destroy();
                            if (p) {
                                if (closeCallback) {
                                    closeCallback({
                                        MMCODE: mmcode,
                                        MMNAME_C: mmname_c,
                                        MMNAME_E: mmname_e,
                                        M_CONTPRICE: m_contprice,
                                        BASE_UNIT: base_unit,
                                        M_DISCPERC: m_discperc,
                                        AGEN_NAMEC: agen_namec
                                    });
                                }
                            }
                        }
                        else
                            Ext.Msg.alert('訊息', '請選取一筆院內碼');
                    }
                },
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        if (p) {
                            if (closeCallback) {
                                closeCallback({
                                    MMCODE: '',
                                    MMNAME_C: '',
                                    MMNAME_E: '',
                                    M_CONTPRICE: '',
                                    BASE_UNIT: '',
                                    M_DISCPERC: '',
                                    AGEN_NAMEC: ''
                                });
                            }
                        }
                    }
                }
            ]
        });
        var win = GetPopWin(viewport, popform, '查詢院內碼', 800, 450);
        if (p.MMCODE != null && p.MMCODE !== '') {
            var f = MmcodeQuery.getForm();
            f.findField('MMCODE1').setValue(p.MMCODE);
        }

    }
    win.show();
}

// 查詢院內碼並選取多筆後回傳院內碼(for 公藥申請) - 湘倫
popMmcodeForm_multi = function (viewport, url, p, closeCallback) {
    var viewModel = Ext.create('WEBAPP.store.MI_MAST_ViewModel');
    var MmcodeStore = viewModel.getStore('MMCODE_MULTI');
    function MmcodeLoad(mmcode, mmcode_c, mmcode_e, e_restrist, p) {
        //MmcodeStore.getProxy().setExtraParam("MAT_CLASS", p.MAT_CLASS);
        //MmcodeStore.getProxy().setExtraParam("WH_NO", p.WH_NO);
        //MmcodeStore.getProxy().setExtraParam("IS_INV", p.IS_INV);
        //MmcodeStore.getProxy().setExtraParam("E_IFPUBLIC", p.E_IFPUBLIC); // 是否公藥  0-非公藥, 1-存點為病房，上級庫為住院藥局 PH1A, 2-存點為病房，上級庫為藥庫 PH1S, 3-存點為病房，設為備用藥，上級庫為住院藥局

        Ext.apply(MmcodeStore.getProxy().extraParams, p);   // 由外部參數傳入
        MmcodeStore.getProxy().setExtraParam("MMCODE", mmcode);
        MmcodeStore.getProxy().setExtraParam("MMNAME_C", mmcode_c);
        MmcodeStore.getProxy().setExtraParam("MMNAME_E", mmcode_e);
        MmcodeStore.getProxy().setExtraParam("E_RESTRICTCODE", e_restrist);
        MmcodeStore.getProxy().setUrl(url);
        MmcodeTool.moveFirst();
    }

    var st_restrict = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0012/GetRestrictCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var MmcodeQuery = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            //padding: '4 0 4 0'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    name: 'MMCODE1',
                    fieldLabel: '院內碼',
                    labelAlign: 'right',
                    enforceMaxLength: true,
                    maxLength: 13,
                    labelWidth: 60,
                    width: 160,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_C',
                    fieldLabel: '中文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 160,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_E',
                    fieldLabel: '英文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 160,
                }, {
                    xtype: 'combo',
                    fieldLabel: '管制用藥',
                    name: 'E_RESTRICTCODE',
                    store: st_restrict,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE'
                },
                {
                    xtype: 'button',
                    id: 'btnSearch',
                    text: '查詢',
                    handler: function () {
                        var f = MmcodeQuery.getForm();
                        MmcodeLoad(f.findField('MMCODE1').getValue(), f.findField('MMNAME_C').getValue(), f.findField('MMNAME_E').getValue(), f.findField('E_RESTRICTCODE').getValue(), p);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('MMCODE1').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var MmcodeTool = Ext.create('Ext.PagingToolbar', {
        store: MmcodeStore,
        displayInfo: true,
        border: false,
        plain: true
    });

    var MmcodeGrid = Ext.create('Ext.grid.Panel', {
        store: MmcodeStore,
        autoScroll: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [MmcodeQuery]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [MmcodeTool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        //viewConfig: {
        //    listeners: {
        //        // 讓column寬度可以隨著內容自動縮放
        //        refresh: function (dataview) {
        //            Ext.each(dataview.panel.columns, function (column) {
        //                if (column.autoSizeColumn === true)
        //                    column.autoSize();
        //            })
        //        }
        //    }
        //},
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                align: 'left',
                //style: 'text-align:center',
                width: 70,
                locked: true
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                align: 'left',
                //style: 'text-align:center',
                //autoSizeColumn: true
                width: 300,
                locked: true
            },
            //{
            //    text: "中文品名",
            //    dataIndex: 'MMNAME_C',
            //    align: 'left',
            //    //style: 'text-align:center',
            //    //autoSizeColumn: true
            //    width: 300
            //},
            {
                text: "管制用藥",
                dataIndex: 'E_RESTRICTCODE',
                align: 'left',
                style: 'text-align:left',
                width: 100
            }, {
                text: "安全量",
                dataIndex: 'SAFE_QTY',
                align: 'right',
                width: 70
            },
            {
                text: "基準量",
                dataIndex: 'OPER_QTY',
                align: 'right',
                width: 70
            },
            {
                text: "扣庫單位",
                dataIndex: 'BASE_UNIT',
                align: 'left',
                width: 70
            },
            {
                text: "現有庫存量",
                dataIndex: 'TO_INV_QTY',
                align: 'right',
                width: 90
            },
            {
                text: "核撥庫房庫存量",
                dataIndex: 'FR_INV_QTY',
                align: 'right',
                width: 120
            },
            {
                text: "核撥庫房",
                dataIndex: 'SUPPLY_WHNO',
                width: 120
            },
            {
                text: "未核撥數量",
                dataIndex: 'APP_QTY_NOT_APPROVED',
                align: 'right',
                width: 90
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            //height: '100%',
            //layout: 'fit',
            //width: 300,
            //height: 300,
            closable: false,
            autoScroll: true,
            items: [
                {
                    xtype: 'container',
                    layout: 'fit',
                    width: 980,
                    height: 380,
                    items: [MmcodeGrid]
                }
            ],
            buttons: [
                {
                    //id: 'winclosed',
                    disabled: false,
                    text: '選取',
                    handler: function () {
                        var selection = MmcodeGrid.getSelection();
                        if (selection.length) {
                            let mmcode = '', mmname_c = '', mmname_e = '', m_contprice = '', base_unit = '', m_discperc = '', agen_namec = '';
                            // selection.map這段在IE不支援,三總初期雖說主要會使用CHROME,
                            // 但後續仍有可能使用舊型PDA是搭載IE,且壓力測試腳本之前也是在IE環境進行,
                            // 改為使用$.map
                            //selection.map(item => {
                            //    mmcode += item.get('MMCODE') + ',';
                            //    //mmname_c += item.get('MMNAME_C') + ',';
                            //    //mmname_e += item.get('MMNAME_E') + ',';
                            //    //m_contprice += item.get('M_CONTPRICE') + ',';
                            //    //base_unit += item.get('BASE_UNIT') + ',';
                            //    //m_discperc += item.get('M_DISCPERC') + ',';
                            //    //agen_namec += item.get('AGEN_NAMEC') + ',';
                            //});
                            $.map(selection, function (item, key) {
                                mmcode += item.get('MMCODE') + ',';
                            })

                            this.up('window').destroy();
                            if (p) {
                                if (closeCallback) {
                                    closeCallback({
                                        MMCODE: mmcode,
                                        //MMNAME_C: mmname_c,
                                        //MMNAME_E: mmname_e,
                                        //M_CONTPRICE: m_contprice,
                                        //BASE_UNIT: base_unit,
                                        //M_DISCPERC: m_discperc,
                                        //AGEN_NAMEC: agen_namec
                                    });
                                }
                            }
                        }
                        else
                            Ext.Msg.alert('訊息', '請選取一筆院內碼');
                    }
                },
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        if (p) {
                            if (closeCallback) {
                                closeCallback({
                                    MMCODE: '',
                                    MMNAME_C: '',
                                    MMNAME_E: '',
                                    M_CONTPRICE: '',
                                    BASE_UNIT: '',
                                    M_DISCPERC: '',
                                    AGEN_NAMEC: ''
                                });
                            }
                        }
                    }
                }
            ]
        });
        var win = GetPopWin(viewport, popform, '查詢院內碼', 1000, 450);
        if (p.MMCODE != null && p.MMCODE !== '') {
            var f = MmcodeQuery.getForm();
            f.findField('MMCODE1').setValue(p.MMCODE);
        }

    }
    win.show();
}


// 查詢院內碼並選取後回傳院內碼FOR AB0003,4,5,6 - 易展
popMmcodeForm_AB0004 = function (viewport, url, p, closeCallback) {
    var viewModel = Ext.create('WEBAPP.store.MI_MAST_ViewModel');
    var MmcodeStore = viewModel.getStore('MMCODE');
    function MmcodeLoad(mmcode, mmcode_c, mmcode_e, p) {
        //MmcodeStore.getProxy().setExtraParam("MAT_CLASS", p.MAT_CLASS);
        //MmcodeStore.getProxy().setExtraParam("WH_NO", p.WH_NO);
        //MmcodeStore.getProxy().setExtraParam("IS_INV", p.IS_INV);
        //MmcodeStore.getProxy().setExtraParam("E_IFPUBLIC", p.E_IFPUBLIC); // 是否公藥  0-非公藥, 1-存點為病房，上級庫為住院藥局 PH1A, 2-存點為病房，上級庫為藥庫 PH1S, 3-存點為病房，設為備用藥，上級庫為住院藥局

        Ext.apply(MmcodeStore.getProxy().extraParams, p);   // 由外部參數傳入
        MmcodeStore.getProxy().setExtraParam("MMCODE", mmcode);
        MmcodeStore.getProxy().setExtraParam("MMNAME_C", mmcode_c);
        MmcodeStore.getProxy().setExtraParam("MMNAME_E", mmcode_e);
        MmcodeStore.getProxy().setUrl(url);
        MmcodeTool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var MmcodeQuery = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            //padding: '4 0 4 0'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    name: 'MMCODE1',
                    fieldLabel: '院內碼',
                    labelAlign: 'right',
                    enforceMaxLength: true,
                    maxLength: 13,
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_C',
                    fieldLabel: '中文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_E',
                    fieldLabel: '英文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'button',
                    id: 'btnSearch',
                    text: '查詢',
                    handler: function () {
                        var f = MmcodeQuery.getForm();
                        if (!f.findField('MMCODE1').getValue() &&
                            !f.findField('MMNAME_C').getValue() &&
                            !f.findField('MMNAME_E').getValue()) {
                            Ext.Msg.alert('提醒', '請至少填入一個查詢條件');
                            return;
                        }
                        MmcodeLoad(f.findField('MMCODE1').getValue(), f.findField('MMNAME_C').getValue(), f.findField('MMNAME_E').getValue(), p);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('MMCODE1').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var MmcodeTool = Ext.create('Ext.PagingToolbar', {
        store: MmcodeStore,
        displayInfo: true,
        border: false,
        plain: true
    });

    var MmcodeGrid = Ext.create('Ext.grid.Panel', {
        store: MmcodeStore,
        autoScroll: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [MmcodeQuery]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [MmcodeTool]
            }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'SINGLE'
        //},
        //selType: 'checkboxmodel',
        //viewConfig: {
        //    listeners: {
        //        // 讓column寬度可以隨著內容自動縮放
        //        refresh: function (dataview) {
        //            Ext.each(dataview.panel.columns, function (column) {
        //                if (column.autoSizeColumn === true)
        //                    column.autoSize();
        //            })
        //        }
        //    }
        //},
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                align: 'left',
                //style: 'text-align:center',
                width: 60
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                align: 'left',
                //style: 'text-align:center',
                //autoSizeColumn: true
                width: 100
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                align: 'left',
                //style: 'text-align:center',
                //autoSizeColumn: true
                width: 250
            },
            {
                text: "庫備識別碼",
                dataIndex: 'M_STOREID',
                align: 'left',
                width: 100
            },
            {
                text: "合約識別碼",
                dataIndex: 'M_CONTID',
                align: 'left',
                width: 100
            },
            {
                text: "申請申購識別碼",
                dataIndex: 'M_APPLYID',
                align: 'left',
                width: 100
            },
            {
                text: "進貨單價",
                dataIndex: 'M_CONTPRICE',
                align: 'left',
                width: 100
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                align: 'left',
                width: 100
            },
            {
                text: "庫房名稱",
                dataIndex: 'AGEN_NAMEC',
                hidden: true
            },
            {
                text: "進貨單價",
                dataIndex: 'M_DISCPERC',
                hidden: true
            },
            {
                text: "計費方式",
                dataIndex: 'M_PAYID',
                hidden: true
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                if (records[0]) {
                    var1 = records[0].data.M_PAYID;
                }
            }
        }
    });

    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            //height: '100%',
            //layout: 'fit',
            width: 400,
            height: 300,
            closable: false,
            autoScroll: true,
            items: [MmcodeGrid],
            buttons: [
                {
                    //id: 'winclosed',
                    disabled: false,
                    text: '選取',
                    handler: function () {
                        var selection = MmcodeGrid.getSelection();
                        if (selection.length) {
                            if (var1 != 'X') {

                                let mmcode = '', mmname_c = '', mmname_e = '', m_contprice = '', base_unit = '', m_discperc = '', agen_namec = '', m_payid = '';
                                // selection.map這段在IE不支援,三總初期雖說主要會使用CHROME,
                                // 但後續仍有可能使用舊型PDA是搭載IE,且壓力測試腳本之前也是在IE環境進行,
                                // 改為使用$.map
                                //selection.map(item => {
                                //    mmcode = item.get('MMCODE');
                                //    mmname_c = item.get('MMNAME_C');
                                //    mmname_e = item.get('MMNAME_E');
                                //    m_contprice = item.get('M_CONTPRICE');
                                //    base_unit = item.get('BASE_UNIT');
                                //    m_discperc = item.get('M_DISCPERC');
                                //    agen_namec = item.get('AGEN_NAMEC');
                                //});
                                $.map(selection, function (item, key) {
                                    mmcode = item.get('MMCODE');
                                    mmname_c = item.get('MMNAME_C');
                                    mmname_e = item.get('MMNAME_E');
                                    m_contprice = item.get('M_CONTPRICE');
                                    base_unit = item.get('BASE_UNIT');
                                    m_discperc = item.get('M_DISCPERC');
                                    agen_namec = item.get('AGEN_NAMEC');
                                    m_payid = item.get('M_PAYID');
                                })

                                this.up('window').destroy();
                                if (p) {
                                    if (closeCallback) {
                                        closeCallback({
                                            MMCODE: mmcode,
                                            MMNAME_C: mmname_c,
                                            MMNAME_E: mmname_e,
                                            M_CONTPRICE: m_contprice,
                                            BASE_UNIT: base_unit,
                                            M_DISCPERC: m_discperc,
                                            AGEN_NAMEC: agen_namec,
                                            M_PAYID: m_payid
                                        });
                                    }
                                }
                            }
                            else {
                                Ext.Msg.alert('訊息', '此院內碼不適用此程式申請');
                            }

                        }
                        else {
                            Ext.Msg.alert('訊息', '請選取一筆院內碼');
                        }
                    }
                },
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        if (p) {
                            if (closeCallback) {
                                closeCallback({
                                    MMCODE: '',
                                    MMNAME_C: '',
                                    MMNAME_E: '',
                                    M_CONTPRICE: '',
                                    BASE_UNIT: '',
                                    M_DISCPERC: '',
                                    AGEN_NAMEC: '',
                                    M_PAYID: ''
                                });
                            }
                        }
                    }
                }
            ]
        });
        var win = GetPopWin(viewport, popform, '查詢院內碼', 800, 450);
        if (p.MMCODE != null && p.MMCODE !== '') {
            var f = MmcodeQuery.getForm();
            f.findField('MMCODE1').setValue(p.MMCODE);
        }

    }
    win.show();
}

// for 國軍推廣案
popMmcodeForm_14 = function (viewport, url, p, closeCallback) {
    var viewModel = Ext.create('WEBAPP.store.MI_MAST_ViewModel');
    var MmcodeStore = viewModel.getStore('MMCODE');
    function MmcodeLoad(mmcode, mmcode_c, mmcode_e, m_agenno, agen_name, mmcode_q1, mmcode_q2, drugsname, p) {
        Ext.apply(MmcodeStore.getProxy().extraParams, p);   // 由外部參數傳入
        MmcodeStore.getProxy().setExtraParam("MMCODE", mmcode);
        MmcodeStore.getProxy().setExtraParam("MMNAME_C", mmcode_c);
        MmcodeStore.getProxy().setExtraParam("MMNAME_E", mmcode_e);
        MmcodeStore.getProxy().setExtraParam("M_AGENNO", m_agenno);
        MmcodeStore.getProxy().setExtraParam("AGEN_NAME", agen_name);
        MmcodeStore.getProxy().setExtraParam("MMCODE_Q1", mmcode_q1);
        MmcodeStore.getProxy().setExtraParam("MMCODE_Q2", mmcode_q2);
        MmcodeStore.getProxy().setExtraParam("DRUGSNAME", drugsname);
        MmcodeStore.getProxy().setUrl(url);
        MmcodeTool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var MmcodeQuery = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            //padding: '4 0 4 0'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    name: 'MMCODE1',
                    fieldLabel: '院內碼',
                    labelAlign: 'right',
                    enforceMaxLength: true,
                    maxLength: 13,
                    labelWidth: 60,
                    width: 150,
                }, {
                    xtype: 'checkbox',
                    name: 'MMCODE_Q1',
                    width: 100,
                    boxLabel: '%院內碼',
                    inputValue: 'Y',
                    checked: true,
                    padding: '0 4 0 8'
                }, {
                    xtype: 'checkbox',
                    name: 'MMCODE_Q2',
                    width: 100,
                    boxLabel: '院內碼%',
                    inputValue: 'Y',
                    checked: true,
                    padding: '0 4 0 8'
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_C',
                    fieldLabel: '中文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_E',
                    fieldLabel: '英文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'DRUGSNAME',
                    fieldLabel: '學名',
                    labelAlign: 'right',
                    labelWidth: 40,
                    width: 180,
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    name: 'M_AGENNO',
                    fieldLabel: '廠商代碼',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 150,
                },
                {
                    xtype: 'textfield',
                    name: 'AGEN_NAME',
                    fieldLabel: '廠商名稱',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'button',
                    id: 'btnSearch',
                    text: '查詢',
                    handler: function () {
                        var f = MmcodeQuery.getForm();
                        MmcodeLoad(f.findField('MMCODE1').getValue(), f.findField('MMNAME_C').getValue(), f.findField('MMNAME_E').getValue(),
                            f.findField('M_AGENNO').getValue(), f.findField('AGEN_NAME').getValue(),
                            f.findField('MMCODE_Q1').getValue(), f.findField('MMCODE_Q2').getValue(),
                            f.findField('DRUGSNAME').getValue(), p);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('MMCODE1').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var MmcodeTool = Ext.create('Ext.PagingToolbar', {
        store: MmcodeStore,
        displayInfo: true,
        border: false,
        plain: true
    });

    var MmcodeGrid = Ext.create('Ext.grid.Panel', {
        store: MmcodeStore,
        autoScroll: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: (viewport.height - 50) * 0.85,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [MmcodeQuery]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [MmcodeTool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                align: 'left',
                width: 100
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                align: 'left',
                width: 150
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                align: 'left',
                width: 250
            },
            {
                text: "庫備識別碼",
                dataIndex: 'M_STOREID',
                align: 'left',
                width: 100
            },
            {
                text: "合約識別碼",
                dataIndex: 'M_CONTID',
                align: 'left',
                width: 100
            },
            {
                text: "申請申購識別碼",
                dataIndex: 'M_APPLYID',
                align: 'left',
                width: 100
            },
            {
                text: "進貨單價",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                align: 'left',
                width: 100
            },
            {
                text: "廠商代碼",
                dataIndex: 'M_AGENNO',
                align: 'left',
                width: 90
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                align: 'left',
                width: 180
            },
            {
                text: "學名",
                dataIndex: 'DRUGSNAME'
            },
            {
                text: "廠商英文名稱",
                dataIndex: 'AGEN_NAMEE',
                hidden: true
            },
            {
                text: "進貨單價",
                dataIndex: 'M_DISCPERC',
                hidden: true
            },
            {
                text: "計費方式",
                dataIndex: 'M_PAYID',
                hidden: true
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                if (records[0]) {
                    var1 = records[0].data.M_PAYID;
                }
            }
        }
    });

    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            //height: '100%',
            //layout: 'fit',
            width: 900,
            height: 450,
            closable: false,
            autoScroll: true,
            items: [MmcodeGrid],
            buttons: [
                {
                    //id: 'winclosed',
                    disabled: false,
                    text: '選取',
                    handler: function () {
                        var selection = MmcodeGrid.getSelection();
                        if (selection.length) {
                            let mmcode = '', mmname_c = '', mmname_e = '', m_contprice = '', base_unit = '', m_discperc = '', agen_namec = '', m_payid = '';
                            $.map(selection, function (item, key) {
                                mmcode = item.get('MMCODE');
                                mmname_c = item.get('MMNAME_C');
                                mmname_e = item.get('MMNAME_E');
                                m_contprice = item.get('M_CONTPRICE');
                                base_unit = item.get('BASE_UNIT');
                                m_discperc = item.get('M_DISCPERC');
                                agen_namec = item.get('AGEN_NAMEC');
                                m_payid = item.get('M_PAYID');
                            })

                            this.up('window').destroy();
                            if (p) {
                                if (closeCallback) {
                                    closeCallback({
                                        MMCODE: mmcode,
                                        MMNAME_C: mmname_c,
                                        MMNAME_E: mmname_e,
                                        M_CONTPRICE: m_contprice,
                                        BASE_UNIT: base_unit,
                                        M_DISCPERC: m_discperc,
                                        AGEN_NAMEC: agen_namec,
                                        M_PAYID: m_payid
                                    });
                                }
                            }

                        }
                        else {
                            Ext.Msg.alert('訊息', '請選取一筆院內碼');
                        }
                    }
                },
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        if (p) {
                            if (closeCallback) {
                                closeCallback({
                                    MMCODE: '',
                                    MMNAME_C: '',
                                    MMNAME_E: '',
                                    M_CONTPRICE: '',
                                    BASE_UNIT: '',
                                    M_DISCPERC: '',
                                    AGEN_NAMEC: '',
                                    M_PAYID: ''
                                });
                            }
                        }
                    }
                }
            ]
        });
        var win = GetPopWin(viewport, popform, '查詢院內碼', viewport.width - 30, viewport.height - 50);

        var f = MmcodeQuery.getForm();
        if (p.MMCODE != null && p.MMCODE !== '') {
            f.findField('MMCODE1').setValue(p.MMCODE);
        }
        // 是否要顯示院內碼的查詢規則選項
        if (p.IS_MMCODEQ == 'Y') {
            f.findField('MMCODE_Q1').setVisible(true);
            f.findField('MMCODE_Q2').setVisible(true);
        }
        else {
            f.findField('MMCODE_Q1').setVisible(false);
            f.findField('MMCODE_Q2').setVisible(false);
        }
        // 是否要顯示學名條件
        if (p.Q_DRUGSNAME == 'Y') {
            f.findField('DRUGSNAME').setVisible(true);
            // 顯示Grid的學名
            for (var i = 0; i < MmcodeGrid.columns.length; i++) {
                if (MmcodeGrid.columns[i].dataIndex == "DRUGSNAME") {
                    MmcodeGrid.columns[i].setVisible(true);
                }
            }
        }
        else {
            f.findField('DRUGSNAME').setVisible(false);
            // 隱藏Grid的學名
            for (var i = 0; i < MmcodeGrid.columns.length; i++) {
                if (MmcodeGrid.columns[i].dataIndex == "DRUGSNAME") {
                    MmcodeGrid.columns[i].setVisible(false);
                }
            }
        }
    }
    win.show();
}

// for 國軍推廣案維護批號效期處理數量(配合批號效期先進先出及儲位出入庫)
popExpForm_14 = function (viewport, url, p, btnText, closeCallback) {
    Ext.define('popExpForm_14Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'EXPDATE', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'STORE_LOC', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'UP', type: 'string' }
        ]
    });
    var MmcodeStore = Ext.create('Ext.data.Store', {
        model: 'popExpForm_14Model',
        pageSize: 200, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }, { property: 'EXPDATE', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function MmcodeLoad(p) {
        Ext.apply(MmcodeStore.getProxy().extraParams, p);   // 由外部參數傳入
        MmcodeStore.getProxy().setUrl(url);
        MmcodeTool.moveFirst();
    }

    var MmcodeTool = Ext.create('Ext.PagingToolbar', {
        store: MmcodeStore,
        displayInfo: true,
        border: false,
        plain: true
    });

    var MmcodeGrid = Ext.create('Ext.grid.Panel', {
        store: MmcodeStore,
        autoScroll: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: (viewport.height - 50) * 0.85,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [MmcodeTool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                align: 'left',
                width: 180,
                sortable: false
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                align: 'left',
                width: 100,
                sortable: false
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                align: 'left',
                width: 150,
                sortable: false
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                align: 'left',
                width: 110,
                sortable: false
            },
            {
                text: "效期",
                dataIndex: 'EXPDATE',
                align: 'left',
                width: 80,
                sortable: false,
                renderer: function (val, meta, record) {
                    if (record.data['UP'] == 'Y')
                        return '<font color=red>' + record.data['EXPDATE'] + '</font>'; // 效期小於180日則UP=Y,以紅字顯示
                    else
                        return record.data['EXPDATE'];
                }
            },
            {
                text: "庫存量",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80,
                sortable: false
            },
            {
                text: "<b><font color=red>" + btnText + "量</font></b>",
                dataIndex: 'APPQTY',
                style: 'text-align:left',
                width: 100,
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/ // 用正規表示式限制可輸入內容
                }, align: 'right',
                sortable: false
            },
            {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                style: 'text-align:left',
                width: 100,
                sortable: false
            },
            {
                header: "",
                flex: 1
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {
                    },
                    validateedit: function (editor, context, eOpts) {
                    }
                }
            })
        ],
        listeners: {
            selectionchange: function (model, records) {
                if (records[0]) {

                }
            }
        }
    });

    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            //height: '100%',
            //layout: 'fit',
            width: 900,
            height: 450,
            closable: false,
            autoScroll: true,
            items: [MmcodeGrid],
            buttons: [
                {
                    disabled: false,
                    text: btnText, // 依功能指定使用的按鈕名稱
                    handler: function () {
                        var store = MmcodeGrid.getStore().data.items;
                        var list = [];
                        var mmcode_sum = {};
                        for (var i = 0; i < store.length; i++) {
                            if (store[i].data.APPQTY > 0) {
                                var item = {
                                    DOCNO: store[i].data.DOCNO,
                                    MMCODE: store[i].data.MMCODE,
                                    APPQTY: store[i].data.APPQTY,
                                    LOT_NO: store[i].data.LOT_NO,
                                    EXPDATE: store[i].data.EXPDATE,
                                    STORE_LOC: store[i].data.STORE_LOC,
                                    FRWH: store[i].data.FRWH,
                                    TOWH: store[i].data.TOWH
                                }
                                list.push(item);
                            }
                            // 統計各單號+院內碼的數量總和
                            if ((store[i].data.DOCNO + store[i].data.MMCODE) in mmcode_sum)
                                mmcode_sum[store[i].data.DOCNO + store[i].data.MMCODE] = parseInt(mmcode_sum[store[i].data.DOCNO + store[i].data.MMCODE]) + parseInt(store[i].data.APPQTY);
                            else
                                mmcode_sum[store[i].data.DOCNO + store[i].data.MMCODE] = parseInt(store[i].data.APPQTY);
                        }

                        // 依基隆需求,批號效期與各單位庫存量需嚴格匹配,改為若各批號總量與申請量不符則不可送出
                        for (var i = 0; i < store.length; i++) {
                            if ((store[i].data.DOCNO + store[i].data.MMCODE) in mmcode_sum) {
                                if (store[i].data.APVQTY_C != mmcode_sum[store[i].data.DOCNO + store[i].data.MMCODE]) {
                                    Ext.Msg.alert('訊息', '單號' + store[i].data.DOCNO + ' 院內碼' + store[i].data.MMCODE + '<br>各批號效期總量' + mmcode_sum[store[i].data.DOCNO + store[i].data.MMCODE] + '不等於申請量' + store[i].data.APVQTY_C + '，請確認!');
                                    return;
                                }
                            }
                        }

                        //if (list.length == 0) {
                        //    Ext.Msg.alert('訊息', '沒有需處理的批號效期數量，請確認!');
                        //    return;
                        //}

                        //單筆撥發用
                        if (p.SEQ != "") {
                            var docMsg = p.DOCNO.replaceAll(',', '<br>');
                            Ext.MessageBox.confirm(btnText, '請確認是否' + btnText + '？' + '單號 如下<br>' + docMsg, function (btn, text) {
                                if (btn === 'yes') {
                                    popform.up('window').destroy();
                                    if (p) {
                                        if (closeCallback) {
                                            closeCallback({
                                                DOCNO: p.DOCNO,
                                                SEQ:p.SEQ,
                                                DATA_LIST: list
                                            });
                                        }
                                    }
                                }
                            });
                        } else {
                            var docMsg = p.DOCNO.replaceAll(',', '<br>');
                            Ext.MessageBox.confirm(btnText, '請確認是否' + btnText + '？' + '單號如下<br>' + docMsg, function (btn, text) {
                                if (btn === 'yes') {
                                    popform.up('window').destroy();
                                    if (p) {
                                        if (closeCallback) {
                                            closeCallback({
                                                DOCNO: p.DOCNO,
                                                DATA_LIST: list
                                            });
                                        }
                                    }
                                }
                            });
                        }
                    }
                },
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }
            ]
        });
        var win = GetPopWin(viewport, popform, '批號效期數量維護', viewport.width - 30, viewport.height - 50);
    }
    win.show();

    MmcodeLoad(p);
}

var winCamera = null;
var showScanWin = function (viewport) {
    if (!winCamera) {
        var popform = Ext.create('Ext.form.Panel', {
            id: 'iframeCamera',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="/Scripts/app/utils/barcode/Barcode.html" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
            buttonAlign: 'center',
            buttons: [{
                text: '<font size="3vmin">返回</font>',
                id: 'winclosed',
                handler: function () {
                    winCamera = null;
                    this.up('window').destroy();
                }
            }]
        });
        winCamera = GetPopWin(viewport, popform, '', viewport.width, viewport.height);
    }
    winCamera.show();
}

// 查詢庫房代碼並選取後回傳庫房代碼 - 俊維
popWh_noForm = function (viewport, url, p, closeCallback) {
    var viewModel = Ext.create('WEBAPP.store.MI_WHMAST_ViewModel');
    var Wh_noStore = viewModel.getStore('WH_NO');
    var var1 = "";
    function Wh_noLoad(wh_no, wh_name, p) {
        //MmcodeStore.getProxy().setExtraParam("MAT_CLASS", p.MAT_CLASS);
        //MmcodeStore.getProxy().setExtraParam("WH_NO", p.WH_NO);
        //MmcodeStore.getProxy().setExtraParam("IS_INV", p.IS_INV);
        //MmcodeStore.getProxy().setExtraParam("E_IFPUBLIC", p.E_IFPUBLIC); // 是否公藥  0-非公藥, 1-存點為病房，上級庫為住院藥局 PH1A, 2-存點為病房，上級庫為藥庫 PH1S, 3-存點為病房，設為備用藥，上級庫為住院藥局

        Ext.apply(Wh_noStore.getProxy().extraParams, p);   // 由外部參數傳入
        Wh_noStore.getProxy().setExtraParam("WH_NO", wh_no);
        Wh_noStore.getProxy().setExtraParam("WH_NAME", wh_name);
        Wh_noStore.getProxy().setUrl(url);
        Wh_noTool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var Wh_noQuery = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    name: 'WH_NO1',
                    fieldLabel: '庫房代碼',
                    labelAlign: 'right',
                    enforceMaxLength: true,
                    maxLength: 13,
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'WH_NAME',
                    fieldLabel: '庫房名稱',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'button',
                    id: 'btnSearch',
                    text: '查詢',
                    handler: function () {
                        var f = Wh_noQuery.getForm();
                        Wh_noLoad(f.findField('WH_NO1').getValue(), f.findField('WH_NAME').getValue(), p);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('WH_NO1').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var Wh_noTool = Ext.create('Ext.PagingToolbar', {
        store: Wh_noStore,
        displayInfo: true,
        border: false,
        plain: true
    });

    var Wh_noGrid = Ext.create('Ext.grid.Panel', {
        store: Wh_noStore,
        autoScroll: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [Wh_noQuery]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [Wh_noTool]
            }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'SINGLE'
        //},
        //selType: 'checkboxmodel',
        //viewConfig: {
        //    listeners: {
        //        // 讓column寬度可以隨著內容自動縮放
        //        refresh: function (dataview) {
        //            Ext.each(dataview.panel.columns, function (column) {
        //                if (column.autoSizeColumn === true)
        //                    column.autoSize();
        //            })
        //        }
        //    }
        //},
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                align: 'left',
                //style: 'text-align:center',
                width: 70
            },
            {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                align: 'left',
                //style: 'text-align:center',
                //autoSizeColumn: true
                width: 300
            },
            {
                text: "庫房分類",
                dataIndex: 'WH_KIND',
                align: 'left',
                width: 100
            },
            {
                text: "庫房級別",
                dataIndex: 'WH_GRADE',
                align: 'left',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            //height: '100%',
            //layout: 'fit',
            width: 300,
            height: 300,
            closable: false,
            autoScroll: true,
            items: [Wh_noGrid],
            buttons: [
                {
                    //id: 'winclosed',
                    disabled: false,
                    text: '選取',
                    handler: function () {
                        var selection = Wh_noGrid.getSelection();
                        if (selection.length) {
                            let wh_no = '', wh_name = '', wh_kind = '', wh_grade = '';
                            // selection.map這段在IE不支援,三總初期雖說主要會使用CHROME,
                            // 但後續仍有可能使用舊型PDA是搭載IE,且壓力測試腳本之前也是在IE環境進行,
                            // 改為使用$.map
                            //selection.map(item => {
                            //    wh_no = item.get('WH_NO');
                            //    wh_name = item.get('WH_NAME');
                            //    wh_kind = item.get('WH_KIND');
                            //    wh_grade = item.get('WH_GRADE');
                            //});
                            $.map(selection, function (item, key) {
                                wh_no = item.get('WH_NO');
                                wh_name = item.get('WH_NAME');
                                wh_kind = item.get('WH_KIND');
                                wh_grade = item.get('WH_GRADE');
                            })

                            this.up('window').destroy();
                            if (p) {
                                if (closeCallback) {
                                    closeCallback({
                                        WH_NO: wh_no,
                                        WH_NAME: wh_name,
                                        WH_KIND: wh_kind,
                                        WH_GRADE: wh_grade
                                    });
                                }
                            }
                        }
                        else
                            Ext.Msg.alert('訊息', '請選取一筆庫房代碼');
                    }
                },
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        if (p) {
                            if (closeCallback) {
                                closeCallback({
                                    WH_NO: '',
                                    WH_NAME: '',
                                    WH_KIND: '',
                                    WH_GRADE: '',
                                });
                            }
                        }
                    }
                }
            ]
        });
        var win = GetPopWin(viewport, popform, '查詢庫房代碼', 800, 450);
        if (p.WH_NO != null && p.WH_NO !== '') {
            var f = Wh_noQuery.getForm();
            f.findField('WH_NO1').setValue(p.WH_NO);
        }

    }
    win.show();
}


/* showExpWindow:效期管理
   用法: showExpWindow(docno[單據號碼],transfer);
   人員: 林詩音 2019/07/01               
*/
function showExpWindow(docno, transfer, viewport) {
    var pgm = "UT0001";
    if (transfer == "I") pgm = "UT0002";
    var strUrl = "../Index/" + pgm + "?docno=" + docno;
    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            id: 'iframeReport',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
            buttons: [{
                text: '關閉',
                handler: function () {
                    this.up('window').destroy();
                }
            }]
        });
        var win = GetPopWin(viewport, popform, "效期管理", viewport.width - 20, viewport.height - 20);
    }
    win.show();
}


// 查詢批號並選取後回傳批號 - 易展
popLotnoForm = function (viewport, url, p, closeCallback) {
    var viewModel = Ext.create('WEBAPP.store.MI_WEXPINV_ViewModel');
    var LotnoStore = viewModel.getStore('LOT_NO');
    var docno = (p != null) ? ((p.DOCNO == null || p.DOCNO == undefined) ? '' : p.DOCNO)
        : '';
    function LotnoLoad(mmcode, p) {

        if (docno != null || docno != '') {
            LotnoStore.getProxy().setExtraParam("DOCNO", docno);
        }
        Ext.apply(LotnoStore.getProxy().extraParams, p);   // 由外部參數傳入
        LotnoStore.getProxy().setExtraParam("MMCODE", mmcode);
        LotnoStore.getProxy().setUrl(url);
        LotnoTool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var LotnoQuery = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '4 0 4 0'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    name: 'MMCODE1',
                    fieldLabel: '院內碼',
                    labelAlign: 'right',
                    enforceMaxLength: true,
                    maxLength: 13,
                    labelWidth: 60,
                    width: 200,
                    readOnly: true
                },
                {
                    xtype: 'button',
                    id: 'btnSearch',
                    text: '查詢',
                    handler: function () {
                        var f = LotnoQuery.getForm();
                        LotnoLoad(f.findField('MMCODE1').getValue(), p);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('MMCODE1').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var LotnoTool = Ext.create('Ext.PagingToolbar', {
        store: LotnoStore,
        displayInfo: true,
        border: false,
        plain: true
    });

    var LotnoGrid = Ext.create('Ext.grid.Panel', {
        store: LotnoStore,
        autoScroll: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [LotnoQuery]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [LotnoTool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                align: 'left',
                width: 70
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                align: 'left',
                width: 100
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                align: 'left',
                width: 100
            },
            {
                text: "效期數量",
                dataIndex: 'INV_QTY',
                align: 'left',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    if (!lotwin) {
        var popform = Ext.create('Ext.form.Panel', {
            //height: '100%',
            //layout: 'fit',
            width: 300,
            height: 300,
            closable: false,
            autoScroll: true,
            items: [LotnoGrid],
            buttons: [
                {
                    disabled: false,
                    text: '選取',
                    handler: function () {
                        var selection = LotnoGrid.getSelection();
                        if (selection.length) {
                            let lot_no = '', exp_date = '', inv_qty = '';
                            // selection.map這段在IE不支援,三總初期雖說主要會使用CHROME,
                            // 但後續仍有可能使用舊型PDA是搭載IE,且壓力測試腳本之前也是在IE環境進行,
                            // 改為使用$.map
                            //selection.map(item => {
                            //    lot_no = item.get('LOT_NO');
                            //    exp_date = item.get('EXP_DATE');
                            //    inv_qty = item.get('INV_QTY');
                            //});
                            $.map(selection, function (item, key) {
                                lot_no = item.get('LOT_NO');
                                exp_date = item.get('EXP_DATE');
                                inv_qty = item.get('INV_QTY');
                            })

                            this.up('window').destroy();
                            if (p) {
                                if (closeCallback) {
                                    closeCallback({
                                        LOT_NO: lot_no,
                                        EXP_DATE: exp_date,
                                        INV_QTY: inv_qty
                                    });
                                }
                            }
                        }
                        else
                            Ext.Msg.alert('訊息', '請選取一筆批號');
                    }
                },
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        if (p) {
                            if (closeCallback) {
                                closeCallback({
                                    LOT_NO: '',
                                    EXP_DATE: '',
                                    INV_QTY: ''
                                });
                            }
                        }
                    }
                }
            ]
        });
        var lotwin = GetPopWin(viewport, popform, '查詢批號', 800, 450);
        if (p.MMCODE != null && p.MMCODE !== '') {
            var f = LotnoQuery.getForm();
            f.findField('MMCODE1').setValue(p.MMCODE);
        }
        LotnoLoad(LotnoQuery.getForm().findField('MMCODE1').getValue(), p);
    }
    lotwin.show();
}

// 查詢院內碼並選取後回傳院內碼 - 家瑋 20230821 能設通信
popMmcodeFormEC = function (viewport, url, p, closeCallback) {
    debugger
    var viewModel = Ext.create('WEBAPP.store.MI_MAST_ViewModel');
    var MmcodeStore = viewModel.getStore('MMCODE');
    debugger
    var wh_no = (p != null) ? ((p.WH_NO == null || p.WH_NO == undefined) ? '' : p.WH_NO)
        : '';
    var docno = (p != null) ? ((p.DOCNO == null || p.DOCNO == undefined) ? '' : p.DOCNO)
        : '';
    var mat_class = (p != null) ? ((p.MAT_CLASS == null || p.MAT_CLASS == undefined) ? '' : p.MAT_CLASS)
        : '';
    debugger
    function MmcodeLoad(mmcode, mmcode_c, mmcode_e, e_restrist, p) {
        //MmcodeStore.getProxy().setExtraParam("MAT_CLASS", p.MAT_CLASS);
        if (wh_no != null || wh_no != '') {
            MmcodeStore.getProxy().setExtraParam("WH_NO", wh_no);
        }
        if (docno != null || docno != '') {
            MmcodeStore.getProxy().setExtraParam("DOCNO", docno);
        }
        if (mat_class != null || mat_class != '') {
            MmcodeStore.getProxy().setExtraParam("MAT_CLASS", mat_class);
        }

        //MmcodeStore.getProxy().setExtraParam("IS_INV", p.IS_INV);
        //MmcodeStore.getProxy().setExtraParam("E_IFPUBLIC", p.E_IFPUBLIC); // 是否公藥  0-非公藥, 1-存點為病房，上級庫為住院藥局 PH1A, 2-存點為病房，上級庫為藥庫 PH1S, 3-存點為病房，設為備用藥，上級庫為住院藥局

        Ext.apply(MmcodeStore.getProxy().extraParams, p);   // 由外部參數傳入
        MmcodeStore.getProxy().setExtraParam("MMCODE", mmcode);
        MmcodeStore.getProxy().setExtraParam("MMNAME_C", mmcode_c);
        MmcodeStore.getProxy().setExtraParam("MMNAME_E", mmcode_e);
        MmcodeStore.getProxy().setUrl(url);
        MmcodeTool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var MmcodeQuery = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            //padding: '4 0 4 0'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    name: 'MMCODE1',
                    fieldLabel: '院內碼',
                    labelAlign: 'right',
                    enforceMaxLength: true,
                    maxLength: 13,
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_C',
                    fieldLabel: '中文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'textfield',
                    name: 'MMNAME_E',
                    fieldLabel: '英文品名',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width: 200,
                },
                {
                    xtype: 'button',
                    id: 'btnSearch',
                    text: '查詢',
                    handler: function () {
                        var f = MmcodeQuery.getForm();
                        MmcodeLoad(f.findField('MMCODE1').getValue(), f.findField('MMNAME_C').getValue(), f.findField('MMNAME_E').getValue(), p);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('MMCODE1').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var MmcodeTool = Ext.create('Ext.PagingToolbar', {
        store: MmcodeStore,
        displayInfo: true,
        border: false,
        plain: true
    });

    var MmcodeGrid = Ext.create('Ext.grid.Panel', {
        store: MmcodeStore,
        autoScroll: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [MmcodeQuery]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [MmcodeTool]
            }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'SINGLE'
        //},
        //selType: 'checkboxmodel',
        //viewConfig: {
        //    listeners: {
        //        // 讓column寬度可以隨著內容自動縮放
        //        refresh: function (dataview) {
        //            Ext.each(dataview.panel.columns, function (column) {
        //                if (column.autoSizeColumn === true)
        //                    column.autoSize();
        //            })
        //        }
        //    }
        //},
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                align: 'left',
                //style: 'text-align:center',
                width: 70
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                align: 'left',
                //style: 'text-align:center',
                //autoSizeColumn: true
                width: 300
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                align: 'left',
                //style: 'text-align:center',
                //autoSizeColumn: true
                width: 300
            },
            {
                text: "進貨單價",
                dataIndex: 'M_CONTPRICE',
                align: 'left',
                width: 100
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                align: 'left',
                width: 100
            },
            {
                text: "庫房名稱",
                dataIndex: 'AGEN_NAMEC',
                hidden: true
            },
            {
                text: "進貨單價",
                dataIndex: 'M_DISCPERC',
                hidden: true
            },
            {
                text: "計費方式",
                dataIndex: 'M_PAYID',
                hidden: true
            },
            {
                text: "庫備識別碼",
                dataIndex: 'M_STOREID',
                hidden: true
            },
            {
                text: "合約識別碼",
                dataIndex: 'M_CONTID',
                hidden: true
            },
            {
                text: "申請申購識別碼",
                dataIndex: 'M_APPLYID',
                hidden: true
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    if (!win) {
        var popform = Ext.create('Ext.form.Panel', {
            //height: '100%',
            //layout: 'fit',
            width: 300,
            height: 300,
            closable: false,
            autoScroll: true,
            items: [MmcodeGrid],
            buttons: [
                {
                    //id: 'winclosed',
                    disabled: false,
                    text: '選取',
                    handler: function () {
                        var selection = MmcodeGrid.getSelection();
                        if (selection.length) {
                            let mmcode = '', mmname_c = '', mmname_e = '', m_contprice = '', base_unit = '', m_discperc = '', agen_namec = '';
                            // selection.map這段在IE不支援,三總初期雖說主要會使用CHROME,
                            // 但後續仍有可能使用舊型PDA是搭載IE,且壓力測試腳本之前也是在IE環境進行,
                            // 改為使用$.map
                            //selection.map(item => {
                            //    mmcode = item.get('MMCODE');
                            //    mmname_c = item.get('MMNAME_C');
                            //    mmname_e = item.get('MMNAME_E');
                            //    m_contprice = item.get('M_CONTPRICE');
                            //    base_unit = item.get('BASE_UNIT');
                            //    m_discperc = item.get('M_DISCPERC');
                            //    agen_namec = item.get('AGEN_NAMEC');
                            //});
                            $.map(selection, function (item, key) {
                                mmcode = item.get('MMCODE');
                                mmname_c = item.get('MMNAME_C');
                                mmname_e = item.get('MMNAME_E');
                                m_contprice = item.get('M_CONTPRICE');
                                base_unit = item.get('BASE_UNIT');
                                m_discperc = item.get('M_DISCPERC');
                                agen_namec = item.get('AGEN_NAMEC');
                            })

                            this.up('window').destroy();
                            if (p) {
                                if (closeCallback) {
                                    closeCallback({
                                        MMCODE: mmcode,
                                        MMNAME_C: mmname_c,
                                        MMNAME_E: mmname_e,
                                        M_CONTPRICE: m_contprice,
                                        BASE_UNIT: base_unit,
                                        M_DISCPERC: m_discperc,
                                        AGEN_NAMEC: agen_namec
                                    });
                                }
                            }
                        }
                        else
                            Ext.Msg.alert('訊息', '請選取一筆院內碼');
                    }
                },
                {
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        if (p) {
                            if (closeCallback) {
                                closeCallback({
                                    MMCODE: '',
                                    MMNAME_C: '',
                                    MMNAME_E: '',
                                    M_CONTPRICE: '',
                                    BASE_UNIT: '',
                                    M_DISCPERC: '',
                                    AGEN_NAMEC: ''
                                });
                            }
                        }
                    }
                }
            ]
        });
        var win = GetPopWin(viewport, popform, '查詢院內碼', 800, 450);
        if (p.MMCODE != null && p.MMCODE !== '') {
            var f = MmcodeQuery.getForm();
            f.findField('MMCODE1').setValue(p.MMCODE);
        }

    }
    win.show();
}

var entityMap = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;',
    '`': '&#x60;',
    '=': '&#x3D;'
};

function escapeHtml(string) {
    if (string) {
        // 日期可能會用到斜線,先移除不過濾
        return String(string).replace(/[&<>"'`=]/g, function (s) {
            return entityMap[s];
        });
    }
    else
        return string;
}