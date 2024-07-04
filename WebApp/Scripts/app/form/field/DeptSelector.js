Ext.define('MMIS.form.field.DeptSelector', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.deptselector',

    requires: [
        'MMIS.grid.Grid'
    ],

    initComponent: function () {
        Ext.define('DeptSelectorModel', {
            extend: 'Ext.data.Model',
            fields: [
                { name: 'DEPT_NO', type: 'string', displayName: '部門代碼' },
                { name: 'DEPT_NAME', type: 'string', displayName: '部門名稱' }
            ]
        });

        var me = this,
            grid = Ext.create('MMIS.grid.Grid', {

                queryFormConfigs: {
                    items: [
                        { name: 'DEPT_NO', labelWidth: 70 },
                        { name: 'DEPT_NAME', labelWidth: 70 }
                    ]
                },

                toolbarItems: [{
                    text: '挑選',
                    handler: function () {
                        me.onSelect();
                    }
                }],

                taskFlow: 'TraDSDeptSelectorGet',
                model: 'DeptSelectorModel',
                columns: [
                    { dataIndex: 'DEPT_NO' },
                    { dataIndex: 'DEPT_NAME', flex: 1 }
                ],
                sorters: ['DEPT_NO', 'DEPT_NAME']
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
        me.setValue(selection.get('DEPT_NO'));
        me.window.close();
    },

    onTriggerClick: function () {
        var me = this,
            grid = me.grid;

        grid.getSelectionModel().select(grid.getStore().find('DEPT_NO', me.getValue()));
        me.window.show();
    }
});