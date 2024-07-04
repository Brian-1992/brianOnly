Ext.define('MMIS.form.field.FlowUserSelector', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.flowuserselector',

    requires: [
        'MMIS.grid.Grid'
    ],

    initComponent: function () {
        Ext.define('FlowUserSelectorModel', {
            extend: 'Ext.data.Model',
            fields: [
                { name: 'TUSER', type: 'string', displayName: '員工編號' },
                { name: 'UNA', type: 'string', displayName: '中文姓名' },
                { name: 'DEPT_NO', type: 'string', displayName: '部門編號' },
                { name: 'DEPT_NAME', type: 'string', displayName: '部門名稱' }
            ]
        });

        var me = this,
            grid = Ext.create('MMIS.grid.Grid', {

                queryFormConfigs: {
                    items: [
                        { name: 'TUSER', labelWidth: 70 },
                        { name: 'DEPT_NO', value: me.param_dept_no, labelWidth: 70 },
                        { name: 'DEPT_NAME', labelWidth: 70 }
                    ]
                },

                toolbarItems: [{
                    text: '挑選',
                    handler: function () {
                        me.onSelect();
                    }
                }],

                taskFlow: 'FlowUserSelectorGet',
                model: 'FlowUserSelectorModel',
                columns: [
                    { dataIndex: 'TUSER' },
                    { dataIndex: 'UNA', flex: 1 },
                    { dataIndex: 'DEPT_NO', flex: 1 },
                    { dataIndex: 'DEPT_NAME', flex: 1 }
                ],
                sorters: ['TUSER', 'UNA']
            }),
            window = Ext.widget('window', {
                width: Ext.getBody().getViewSize().width,
                height: Ext.getBody().getViewSize().height,
                autoScroll: true,
                constrain: true,
                title: '請選擇人員',
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
        me.setValue(selection.get('TUSER'));
        me.window.close();
    },

    onTriggerClick: function () {
        var me = this,
            grid = me.grid;

        Ext.apply(grid.getStore().getProxy().extraParams, {
            DEPT_NO: me.param_dept_no
        });
        grid.getStore().load();
        grid.getSelectionModel().select(grid.getStore().find('TUSER', me.getValue()));
        me.window.show();
    }
});