Ext.define('MMIS.form.field.MMISDeptSelector', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.mmisdeptselector',

    requires: [
        'MMIS.grid.Grid'
    ],

    initComponent: function () {
        Ext.define('DeptSelectorModel', {
            extend: 'Ext.data.Model',
            fields: [
                { name: 'OuterId', type: 'string', displayName: '部門代碼' },
                { name: 'Dept_Title', type: 'string', displayName: '部門名稱' }
            ]
        });

        var me = this,
            grid = Ext.create('MMIS.grid.Grid', {

                queryFormConfigs: {
                    items: [
                        { name: 'OuterId', labelWidth: 70 },
                        { name: 'Dept_Title', labelWidth: 70 }
                    ]
                },

                toolbarItems: [{
                    text: '挑選',
                    handler: function () {
                        me.onSelect();
                    }
                }],

                taskFlow: 'TraURMMISDeptSelectorDFGet',
                model: 'DeptSelectorModel',
                columns: [
                    { dataIndex: 'OuterId' },
                    { dataIndex: 'Dept_Title', flex: 1 }
                ],
                sorters: ['OuterId', 'Dept_Title']
            }),
            window = Ext.widget('window', {
                width: 800,
                height: 500,
                constrain: true,
                title: '請選擇部門',
                layout: 'fit',
                items: grid,
                closeAction: 'hide'
            });

        Ext.apply(me, {
            window: window,
            grid: grid
        });

        me.callParent(arguments);
    },

    onSelect: function () {
        var me = this,
            selection = me.grid.getSelectionModel().getSelection()[0];
        me.setValue(selection.get('OuterId'));
        me.window.close();
    },

    onTriggerClick: function () {
        var me = this,
            grid = me.grid;

        grid.getSelectionModel().select(grid.getStore().find('OuterId', me.getValue()));
        me.window.show();
    }
});