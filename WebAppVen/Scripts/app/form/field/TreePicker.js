//Fix the double click issue!
Ext.define('MMIS.override.FixTreePicker', {
    override: 'Ext.ux.TreePicker',
    selectItem: function (record) {
        var me = this;
        me.setValue(record.getId());
        //me.picker.hide();
        me.collapse();
        me.inputEl.focus();
        me.fireEvent('select', me, record);
    }
});
Ext.define('MMIS.form.field.TreePicker', {
    extend: 'Ext.ux.TreePicker',
    alias: 'widget.mmistreepicker',

    displayField: 'text',
    
    /**
     * @cfg {String} url
     */

    rootVisable: false,

    maxPickerHeight: 500,

    initComponent: function () {
        Ext.apply(this, {
            store: Ext.create('MMIS.data.TreeStore', {
                autoLoad: true,
                proxy: {
                    type: 'ajax',
                    url: this.url
                }
            })
        });
        this.store.load();
        this.callParent(arguments);
    },

    createPicker: function () {
        var me = this,
            picker = new Ext.tree.Panel({
                shrinkWrapDock: 2,
                store: me.store,
                floating: true,
                displayField: me.displayField,
                columns: me.columns,
                minHeight: me.minPickerHeight,
                maxHeight: me.maxPickerHeight,
                manageHeight: false,
                shadow: false,
                rootVisible: false,
                listeners: {
                    scope: me,
                    itemclick: me.onItemClick
                },
                viewConfig: {
                    listeners: {
                        scope: me,
                        render: me.onViewRender
                    }
                }
            }),
            view = picker.getView();

        if (Ext.isIE9 && Ext.isStrict) {
            // In IE9 strict mode, the tree view grows by the height of the horizontal scroll bar when the items are highlighted or unhighlighted.
            // Also when items are collapsed or expanded the height of the view is off. Forcing a repaint fixes the problem.
            view.on({
                scope: me,
                highlightitem: me.repaintPickerView,
                unhighlightitem: me.repaintPickerView,
                afteritemexpand: me.repaintPickerView,
                afteritemcollapse: me.repaintPickerView
            });
        }
        return picker;
    },
});