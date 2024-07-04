/** 顏色選擇欄位 */
Ext.define('MMIS.form.field.Color', {
    extend: 'Ext.form.field.Picker',

    alias: 'widget.colorfield',

    createPicker: function () {
        var me = this;
        return Ext.create('Ext.picker.Color', {
            style: 'background-color: #FFF',
            pickerField: me,
            floating: true,
            listeners: {
                select: function (picker, selColor) {
                    me.setValue('#' + selColor);
                    Ext.defer(me.collapse, 1, me);
                }
            }
        });
    },

    afterRender: function () {
        var me = this;
        me.showColor(me.value);
        me.callParent(arguments);
    },

    fieldStyle: {
        backgroundImage: 'none'
    },

    checkIsColorCode: function (str) {
        var el = Ext.DomHelper.createDom({ tag: 'div' });

        el = Ext.get(el);
        el.setStyle('background-color', 'white');
        el.setStyle('background-color', str);
        if (el.getStyle('background-color') != 'rgb(255, 255, 255)') {
            return true;
        }
        return false;
    },

    showColor: function (color) {
        var me = this;
        if (me.checkIsColorCode(color)) {
            me.inputEl.setStyle('background-color', color);
        } else {
            me.inputEl.setStyle('background-color', '#FFF');
        }
    },

    listeners: {
        change: function (field, newValue) {
            this.showColor(newValue);
        }
    }
});