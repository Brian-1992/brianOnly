Ext.define('MMIS.form.field.MaterialSelector', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.materialselector',

    requires: [
        'MMIS.grid.Grid'
    ],

    triggerCls: Ext.baseCSSPrefix + 'form-search-trigger',

    storeConfig: {
        taskFlow: 'TraDSMaterialSelectorGet',
        fields: [
            'MATERIAL_NO',
            'MATERIAL_NAME_EN',
            'MATERIAL_NAME_CH',
            'INV_QTY',
            'SF_QTY',
            'LT_QTY',
            'ROP_QTY',
            'Q1_QTY'
        ]
    },

    columns: [
        { dataIndex: 'MATERIAL_NO', text: '材料編號' },
        { dataIndex: 'MATERIAL_NAME_EN', text: '英文名稱', flex: 1 },
        { dataIndex: 'MATERIAL_NAME_CH', text: '中文名稱', flex: 1 }
    ],

    initComponent: function () {
        
        var me = this,
            grid = Ext.create('MMIS.grid.Grid', {
                queryFormConfigs: {
                    items: [
                        { name: 'MATERIAL_NO', fieldLabel: '材料編號', labelWidth: 70 },
                        { name: 'MATERIAL_NAME_CH', fieldLabel: '中文名稱', labelWidth: 70 }
                    ]
                },

                toolbarItems: [{
                    text: '挑選',
                    cls: 'eye-catching-button',
                    handler: function () {
                        me.onSelect();
                    }
                }],
                
                storeConfig: me.storeConfig,
                columns: me.columns
            }),
            window = Ext.widget('window', {
                width: 800,
                height: 500,
                constrain: true,
                title: '請選擇材料',
                layout: 'fit',
                items: grid,
                closeAction: 'hide'
            });

        Ext.apply(me, {
            window: window,
            grid: grid
        });

        me.addEvents('onselect');

        me.callParent(arguments);
    },

    onSelect: function () {
        var me = this,
            selection = me.grid.getSelectionModel().getSelection()[0];
        me.setValue(selection.get('MATERIAL_NO'));
        me.window.close();
        me.fireEvent('onselect', me, selection);
    },

    onTriggerClick: function () {
        var me = this,
            grid = me.grid;

        grid.getSelectionModel().select(grid.getStore().find('MATERIAL_NO', me.getValue()));
        me.window.show();
    }
});
