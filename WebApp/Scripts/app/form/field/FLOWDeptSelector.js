Ext.define('MMIS.form.field.FLOWDeptSelector', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.flowdeptselector',

    requires: [
        'MMIS.grid.Grid'
    ],

    initComponent: function () {
        Ext.define('DeptSelectorModel', {
            extend: 'Ext.data.Model',
            fields: [
                { name: 'MMIS_DEPT_NO', type: 'string', displayName: '部門代碼' },
                { name: 'DEPT_NAME', type: 'string', displayName: '部門名稱' }
            ]
        });

        var me = this,
            grid = Ext.create('MMIS.grid.Grid', {

                queryFormConfigs: {
                    items: [
                        { name: 'OID', id: 'OID', labelWidth: 70, hidden: true },
                        { name: 'MMIS_DEPT_NO', labelWidth: 70 },
                        { name: 'DEPT_NAME', labelWidth: 70 }
                    ]
                },

                toolbarItems: [{
                    text: '挑選',
                    handler: function () {
                        me.onSelect();
                    }
                }],

                taskFlow: 'TraURFLOWDeptSelectorGet',
                model: 'DeptSelectorModel',
                columns: [
                    { dataIndex: 'MMIS_DEPT_NO' },
                    { dataIndex: 'DEPT_NAME', flex: 1 }
                ],
                sorters: ['MMIS_DEPT_NO', 'DEPT_NAME']
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
        me.setValue(selection.get('MMIS_DEPT_NO'));
        me.window.close();
    },

    onTriggerClick: function () {
        var me = this,
            grid = me.grid;

        grid.getSelectionModel().select(grid.getStore().find('MMIS_DEPT_NO', me.getValue()));
        Ext.getCmp('OID').setValue(me.oid);
        me.window.show();
    }
});