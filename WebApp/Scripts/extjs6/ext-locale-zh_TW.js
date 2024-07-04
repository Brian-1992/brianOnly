/**
 * Traditional Chinese translation
 * By hata1234
 * 09 April 2007
 */
Ext.onReady(function() {
    var parseCodes;

    if (Ext.Date) {
        Ext.Date.monthNames = ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"];

        Ext.Date.dayNames = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];

        Ext.Date.formatCodes.a = "(this.getHours() < 12 ? '上午' : '下午')";
        Ext.Date.formatCodes.A = "(this.getHours() < 12 ? '上午' : '下午')";
        Ext.Date.formatCodes.X = "(this.getFullYear() - 1911)"; //民國年 2019/4/17 吉威

        parseCodes = {
            g: 1,
            c: "if (/(上午)/i.test(results[{0}])) {\n"
                + "if (!h || h == 12) { h = 0; }\n"
                + "} else { if (!h || h < 12) { h = (h || 0) + 12; }}",
            s: "(上午|下午)",
            calcAtEnd: true
        };

        Ext.Date.parseCodes.a = Ext.Date.parseCodes.A = parseCodes;

        Ext.Date.parseFunctions['Xmd H'] = function (input, strict) {
            var rocYear, yyyy, mmdd, normalDate, len;
            input = input.substring(0, input.indexOf(' '));
            len = input.length;
            if (len > 4 && len < 8) {
                rocYear = input.substring(0, len - 4); //民國年
                yyyy = parseInt(rocYear) + 1911;
                //mmdd = input.substring(3, 7); // MMDD
                mmdd = input.substring(len - 4, len); // MMDD
                normalDate = Ext.Date.parse(yyyy + mmdd, 'Ymd');
                if (Ext.isDate(normalDate)) {
                    return normalDate;
                } else {
                    return null;
                }
            }
        };
        
        Ext.Date.parseFunctions['Xm'] = function (input, strict) {
            var rocYear, yyyy, mmdd, normalDate, len;
            //input = input.substring(0, input.indexOf(' '));
            len = input.length;
            if (len >= 4 && len < 6) {
                rocYear = input.substring(0, len - 2); //民國年
                yyyy = parseInt(rocYear) + 1911;
                //mmdd = input.substring(3, 7); // MMDD
                mm = input.substring(len - 2, len); // MM
                normalDate = Ext.Date.parse(yyyy + mm + '01', 'Ymd');
                if (Ext.isDate(normalDate)) {
                    return normalDate;
                } else {
                    return null;
                }
            }
        };
    }

    if (Ext.util && Ext.util.Format) {
        Ext.apply(Ext.util.Format, {
            thousandSeparator: ',',
            decimalSeparator: '.',
            currencySign: '\u00a5',
            // Chinese Yuan
            dateFormat: 'Y/m/d'
        });
    }
});

Ext.define("Ext.locale.zh_TW.view.View", {
    override: "Ext.view.View",
    emptyText: ""
});

Ext.define("Ext.locale.zh_TW.grid.plugin.DragDrop", {
    override: "Ext.grid.plugin.DragDrop",
    dragText: "選擇了 {0} 行"
});

Ext.define("Ext.locale.zh_TW.tab.Tab", {
    override: "Ext.tab.Tab",
    closeText: "關閉此標籤"
});

Ext.define("Ext.locale.zh_TW.form.field.Base", {
    override: "Ext.form.field.Base",
    invalidText: "數值不符合欄位規定"
});

// changing the msg text below will affect the LoadMask
Ext.define("Ext.locale.zh_TW.view.AbstractView", {
    override: "Ext.view.AbstractView",
    loadingText: "讀取中..."
});

Ext.define("Ext.locale.zh_TW.picker.Date", {
    override: "Ext.picker.Date",
    todayText: "今天",
    minText: "日期必須大於最小容許日期",
    maxText: "日期必須小於最大容許日期",
    disabledDaysText: "",
    disabledDatesText: "",
    nextText: "下個月 (Ctrl+右方向鍵)",
    prevText: "上個月 (Ctrl+左方向鍵)",
    monthYearText: "選擇月份 (Ctrl+上/下方向鍵選擇年份)",
    format: "y/m/d",
    ariaTitle: '{0}',
    ariaTitleDateFormat: 'Y\u5e74m\u6708d\u65e5',
    longDayFormat: 'Y\u5e74m\u6708d\u65e5',
    //monthYearFormat: 'Y\u5e74m\u6708',
    monthYearFormat: '民國X\u5e74m\u6708',
    getDayInitial: function (value) {
        // Grab the last character
        return value.substr(value.length - 1);
    }
});

Ext.define("Ext.locale.zh_TW.picker.Month", {
    override: "Ext.picker.Month",
    okText: "確定",
    cancelText: "取消",
    updateBody: function () {
        var me = this,
            years = me.years,
            months = me.months,
            yearNumbers = me.getYears(),
            cls = me.selectedCls,
            value = me.getYear(null),
            month = me.value[0],
            monthOffset = me.monthOffset,
            year, yearItems, y, yLen, el;
        if (me.rendered) {
            years.removeCls(cls);
            months.removeCls(cls);
            yearItems = years.elements;
            yLen = yearItems.length;
            for (y = 0; y < yLen; y++) {
                el = Ext.fly(yearItems[y]);
                year = yearNumbers[y];
                el.dom.innerHTML = (year - 1911) + '年';
                if (year === value) {
                    el.addCls(cls);
                }
            }
            if (month !== null) {
                if (month < monthOffset) {
                    month = month * 2;
                } else {
                    month = (month - monthOffset) * 2 + 1;
                }
                months.item(month).addCls(cls);
            }
        }
    }
});

Ext.define("Ext.locale.zh_TW.toolbar.Paging", {
    override: "Ext.PagingToolbar",
    beforePageText: "第",
    afterPageText: "頁，共{0}頁",
    firstText: "第一頁",
    prevText: "上一頁",
    nextText: "下一頁",
    lastText: "最後頁",
    refreshText: "重新整理",
    displayMsg: "顯示{0} - {1}筆,共{2}筆",
    emptyMsg: '沒有任何資料',

    //hide refresh button
    getPagingItems: function () {
        var me = this,
            inputListeners = {
                scope: me,
                blur: me.onPagingBlur
            };
        inputListeners[Ext.supports.SpecialKeyDownRepeat ? 'keydown' : 'keypress'] = me.onPagingKeyDown;
        return [
            {
                itemId: 'first',
                tooltip: me.firstText,
                overflowText: me.firstText,
                iconCls: Ext.baseCSSPrefix + 'tbar-page-first',
                disabled: true,
                handler: me.moveFirst,
                scope: me
            },
            {
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
                // mark it as not a field so the form will not catch it when getting fields
                isFormField: false,
                width: me.inputItemWidth,
                margin: '-1 2 3 2',
                listeners: inputListeners
            },
            {
                xtype: 'tbtext',
                itemId: 'afterTextItem',
                html: Ext.String.format(me.afterPageText, 1)
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
            },
            {
                itemId: 'last',
                tooltip: me.lastText,
                overflowText: me.lastText,
                iconCls: Ext.baseCSSPrefix + 'tbar-page-last',
                disabled: true,
                handler: me.moveLast,
                scope: me
            },
            '-'
            /** hide refresh button 
            ,
            {
                itemId: 'refresh',
                tooltip: me.refreshText,
                overflowText: me.refreshText,
                iconCls: Ext.baseCSSPrefix + 'tbar-loading',
                disabled: me.store.isLoading(),
                handler: me.doRefresh,
                scope: me
            } */
        ];
    }
});

Ext.define("Ext.locale.zh_TW.form.field.Text", {
    override: "Ext.form.field.Text",
    minLengthText: "此欄位最少要輸入 {0} 個字",
    maxLengthText: "此欄位最多輸入 {0} 個字",
    blankText: "此欄位為必填",
    regexText: "",
    emptyText: null
});

Ext.define("Ext.locale.zh_TW.form.field.Number", {
    override: "Ext.form.field.Number",
    minText: "此欄位之數值必須大於 {0}",
    maxText: "此欄位之數值必須小於 {0}",
    nanText: "{0} 不是合法的數字"
});

Ext.define("Ext.locale.zh_TW.form.field.Date", {
    override: "Ext.form.field.Date",
    disabledDaysText: "無法使用",
    disabledDatesText: "無法使用",
    minText: "此欄位之日期必須在 {0} 之後",
    maxText: "此欄位之日期必須在 {0} 之前",
    //invalidText: "{0} 不是正確的日期格式 - 必須像是 「 {1} 」 這樣的格式",
    invalidText: "{0} 不是正確的日期格式 - 必須像是 「 1081016 」 這樣的格式",
    altFormats: 'm/d/Y|n/j/Y|n/j/y|m/j/y|n/d/y|m/j/Y|n/d/Y|m-d-y|m-d-Y|m/d|m-d|md|mdy|mdY|d|Y-m-d|n-j|n/j|Xmd',
    //format: "Y/m/d",
    format: 'Xmd',
    submitFormat: 'Y/m/d'
});

Ext.define("Ext.locale.zh_TW.form.field.ComboBox", {
    override: "Ext.form.field.ComboBox",
    valueNotFoundText: undefined
}, function() {
    Ext.apply(Ext.form.field.ComboBox.prototype.defaultListConfig, {
        loadingText: "讀取中 ..."
    });
});

Ext.define("Ext.locale.zh_TW.form.field.VTypes", {
    override: "Ext.form.field.VTypes",
    emailText: '此欄位必須輸入像 "user@example.com" 之E-Mail格式',
    urlText: '此欄位必須輸入像 "http:/' + '/www.example.com" 之網址格式',
    alphaText: '此欄位僅能輸入半形英文字母及底線( _ )符號',
    alphanumText: '此欄位僅能輸入半形英文字母、數字及底線( _ )符號'
});

Ext.define("Ext.locale.zh_TW.grid.header.Container", {
    override: "Ext.grid.header.Container",
    sortAscText: "正向排序",
    sortDescText: "反向排序",
    lockText: "鎖定欄位",
    unlockText: "解開欄位鎖定",
    columnsText: "欄位"
});

Ext.define("Ext.locale.zh_TW.grid.PropertyColumnModel", {
    override: "Ext.grid.PropertyColumnModel",
    nameText: "名稱",
    valueText: "數值",
    dateFormat: "Y/m/d"
});

Ext.define("Ext.locale.zh_TW.window.MessageBox", {
    override: "Ext.window.MessageBox",
    buttonText: {
        ok: "確定",
        cancel: "取消",
        yes: "是",
        no: "否"
    }    
});

// This is needed until we can refactor all of the locales into individual files
Ext.define("Ext.locale.zh_TW.Component", {	
    override: "Ext.Component"
});
