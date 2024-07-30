/**
 * 年月選擇欄位
 * 
 * 使用範例：
 *
 *     @example
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             { 
 *                 xtype: 'monthfield'
 *             }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 */


Ext.define('MMIS.form.field.Month', {
    extend: 'Ext.form.field.Date',
    alias: 'widget.monthfield',
    requires: ['MMIS.picker.Month'],
    alternateClassName: ['MMIS.form.field.MonthField'],
    selectMonth: null,
    format: 'Y/m',
    createPicker: function () {
        var me = this,
            format = Ext.String.format;
        return Ext.create('MMIS.picker.Month', {

            pickerField: me,
            ownerCt: me.ownerCt,
            floating: true,
            hidden: true,
            focusOnShow: true,
            minValue: me.minValue,
            maxValue: me.maxValue,
            setMinDate: function (v) {
                this.minValue = v;
            },
            setMaxDate: function (v) {
                this.maxValue = v;
            },
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
                select: { scope: me, fn: me.onSelect },
                monthdblclick: { scope: me, fn: me.onOKClick },
                yeardblclick: { scope: me, fn: me.onOKClick },
                OkClick: { scope: me, fn: me.onOKClick },
                CancelClick: { scope: me, fn: me.onCancelClick }
            },
            keyNavConfig: {
                esc: function () {
                    me.collapse();
                }
            }
        });
    },
    onCancelClick: function () {
        var me = this;
        me.selectMonth = null;
        me.collapse();
    },
    onOKClick: function () {
        var me = this;
        if (me.selectMonth) {
            me.setValue(me.selectMonth);
            me.fireEvent('select', me, me.selectMonth);
        }
        me.collapse();
    },
    onSelect: function (m, d) {
        var me = this;
        me.selectMonth = new Date((d[0] + 1) + '/1/' + d[1]);
    }
});