Ext.define('MMIS.form.field.JobTitleSelector', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.jobtitleselector',

    requires: [
        'MMIS.grid.Grid'
    ],

    initComponent: function () {
        Ext.define('DeptSelectorModel', {
            extend: 'Ext.data.Model',
            fields: [
                { name: 'JobTitle_GUID', type: 'string', displayName: 'GUID' },
                { name: 'JobTitle', type: 'string', displayName: '職稱' }
            ]
        });

        var me = this,
            grid = Ext.create('MMIS.grid.Grid', {

                queryFormConfigs: {
                    items: [
                        { name: 'JobTitle', labelWidth: 70 }
                    ]
                },

                toolbarItems: [{
                    text: '挑選',
                    handler: function () {
                        me.onSelect();
                    }
                }],

                taskFlow: 'TraURJobTitleGet',
                model: 'DeptSelectorModel',
                columns: [
                    { dataIndex: 'JobTitle', flex: 1 }
                ],
                sorters: ['JobTitle']
            }),
            window = Ext.widget('window', {
                width: 800,
                height: 500,
                constrain: true,
                title: '請選擇職稱',
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
        me.setValue(selection.get('JobTitle'));
        me.window.close();
    },

    onTriggerClick: function () {
        var me = this,
            grid = me.grid;

        grid.getSelectionModel().select(grid.getStore().find('JobTitle', me.getValue()));
        me.window.show();
    }
});