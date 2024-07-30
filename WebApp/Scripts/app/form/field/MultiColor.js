Ext.define('MMIS.form.field.MultiColor', {
    extend: 'Ext.form.FieldContainer',
    alias: 'widget.multicolorfield',

    requires: ['MMIS.form.field.Color'],

    initComponent: function () {
        var me = this;
        Ext.apply(me, {
            items: [
                {
                    xtype: 'container',
                    layout: 'hbox',
                    defaults: {
                        margin: '0 5 5 0'
                    },
                    width: 300,
                    items: [
                        {
                            xtype: 'colorfield',
                            width: 150
                        },
                        {
                            itemId: 'btnAdd',
                            xtype: 'button',
                            text: '新增',
                            handler: function () {
                                var colorfield = me.down('colorfield');
                                color = colorfield.getValue();
                                if (color) {                                   
                                    me.addColor(color);
                                    colorfield.setValue('');
                                }
                            }
                        },
                        {
                            itemId: 'btnClear',
                            xtype: 'button',
                            text: '清空',
                            handler: function () {
                                me.down('colorfield').setValue('');
                                me.removeAllColor();
                            }
                        }
                    ]
                },
                {
                    itemId: 'colorItemContainer',
                    xtype: 'container',
                    width: 300,
                    height: 50,
                    layout: 'hbox',
                    autoScroll: true,
                    cls: 'x-form-field x-form-text',
                    padding: 5,
                    defaults: {
                        width: 70,
                        height: 30,
                        margin: 3,
                        padding: 3
                    }
                },
                {
                    itemId: 'valueField',
                    xtype: 'hidden',
                    name: me.name,
                    width: 100,
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            me.setColorItems(newValue);
                        }
                    }
                }
            ]
        });

        me.colors = [];

        me.callParent(arguments);

        me.setValue(me.value);
    },

    addColor: function (color) {
        var me = this,
            colorItemContainer = me.down('#colorItemContainer'),
            item = Ext.widget('component', {
                html: color,
                style: {
                    backgroundColor: color,
                    border: '1px solid  #969696'
                }
            });
        colorItemContainer.add(item);
        me.colors.push(color);
        me.syncValueToField();
    },

    removeAllColor: function () {
        var me = this;
        me.down('#colorItemContainer').removeAll();
        me.colors = [];
        me.syncValueToField();
    },

    syncValueToField: function () {
        var me = this,
            value = me.colors.join(','),
            valueField = me.down('#valueField');
        valueField.suspendEvents();
        valueField.setValue(value);
        valueField.resumeEvents();
    },

    setColorItems: function (v) {
        var me = this,
            values = typeof v === 'string' ? v.split(',') : v;

        me.removeAllColor();

        Ext.each(values, function (value) {
            me.addColor(value);
        });
    },

    setValue: function (v) {
        this.down('#valueField').setValue(v);
    },

    getValue: function () {
        return this.colors;
    }
});