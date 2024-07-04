Ext.define('MMIS.picker.Month', {
    extend: 'Ext.picker.Month',

    disabledCellCls: 'monthpicker-disabled-cell',

    initComponent: function () {
        var me = this;
        Ext.util.CSS.createStyleSheet(
            '.x-monthpicker-item .monthpicker-disabled-cell ' +
            '{background-color:#eee;cursor:default;color:#bbb}' +
            '.x-monthpicker-buttons ' +
            '{position: static;}'
            );
        me.callParent(arguments);
    },

    updateDisableCellCls: function () {

        var me = this,
            eDate = Ext.Date,
            minValue = me.minValue,
            maxValue = me.maxValue,

            maxYear = maxValue ? maxValue.getFullYear() : Number.POSITIVE_INFINITY,
            minYear = minValue ? minValue.getFullYear() : Number.NEGATIVE_INFINITY,

            cls = me.disabledCellCls,

            years = me.years,
            months = me.months,
            year,
            month,
            selectedYear = me.getYear(null),
            selectedMonth = me.value[0],
            yearItems, y, yLen,
            monthItems, m, mLen, el;

        if (me.rendered) {
            years.removeCls(cls);
            months.removeCls(cls);

            yearItems = years.elements;
            yLen = yearItems.length;
            for (y = 0; y < yLen; y += 1) {
                el = Ext.fly(yearItems[y]);
                year = me.activeYear + me.resolveOffset(y, me.yearOffset);

                if (year > maxYear || year < minYear) {
                    el.addCls(cls);
                }
            }

            monthItems = months.elements;
            mLen = monthItems.length;
            for (m = 0; m < mLen; m += 1) {
                el = Ext.fly(monthItems[m]);
                month = me.resolveOffset(m, me.monthOffset);

                if (selectedYear === maxYear && month > maxValue.getMonth()) {
                    if (month === selectedMonth) {
                        me.value[0] = maxValue.getMonth();
                        me.updateBody();
                        return;
                    }
                    el.addCls(cls);
                }

                if (selectedYear === minYear && month < minValue.getMonth()) {
                    if (month === selectedMonth) {
                        me.value[0] = minValue.getMonth();
                        me.updateBody();
                        return;
                    }
                    el.addCls(cls);
                }
            }
        }
    },

    updateBody: function () {
        var me = this;
        me.callParent(arguments);
        me.updateDisableCellCls();
    },

    onYearClick: function (target, isDouble) {
        var me = this,
            el = me.years.item(me.years.indexOf(target));

        if (!el.hasCls(me.disabledCellCls)) {
            me.callParent(arguments);
        }
    },

    onMonthClick: function (target, isDouble) {
        var me = this,
            el = me.months.item(me.months.indexOf(target));

        if (!el.hasCls(me.disabledCellCls)) {
            me.callParent(arguments);
        }
    }
});
