Ext.onReady(function () {
    Ext.ariaWarn = Ext.emptyFn;
    var T1Get = '../../../api/UR1012/Show';
    var T1Name = "公佈欄";

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var T1F1 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['ID', 'TITLE', 'CONTENT', 'ON_DATE', 'OFF_DATE', 'CREATE_BY', 'CREATE_DT', 'ATTACH_CNT']
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [{
            fieldLabel: '標題',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 50,
            labelWidth: 50,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '內容',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 50,
            labelWidth: 50,
            padding: '0 4 0 4'
        }, {
            xtype: 'button',
            text: '查詢',
            handler: T1Load
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        autoLoad: true,
        sorters: [{ property: 'ID', direction: 'DESC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST', // by default GET
                update: 'POST',
                destroy: 'POST'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    showUploadWindowL = function (id, title) {
        showUploadWindow(false, false, "URBLT," + id, '公佈欄 - [' + title + '] 附件列表', viewport);
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        viewConfig: {
            getRowClass: function (record, rowIndex, rowParams, store) { return 'multiline-row'; }
        },
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],

        // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "標題",
            dataIndex: 'TITLE',
            width: 150
        }, {
            text: "內容",
            dataIndex: 'CONTENT',
            renderer: function (value, metaData, record) {
                return value.replace(/\n/g,'<br/>');
            },
            flex: 1
        }, {
            text: "附件",
            width: 70,
            align: 'center',
            renderer: function (val, meta, record) {
                var ac = parseInt(record.get("ATTACH_CNT"));
                if (ac > 0)
                    return '<a href=\'javascript:showUploadWindowL(\"' + record.get("ID") + '\", \"' + record.get("TITLE") +'\");\'><img src="../../../Images/TRA/save.gif" align="absmiddle" /></a>';
                //return '<a href=javascript:getFileName("' + record.get('FG') + '");>' + val + '</a>';
                return "";
            }
        }, {
            text: "公告人員",
            dataIndex: 'CREATE_BY',
            width: 100
        }/*, {
            text: "公告時間",
            dataIndex: 'CREATE_DT',
            width: 150
        }*/, {
            text: "起始日期",
            dataIndex: 'ON_DATE',
            width: 90
        }, {
            text: "結束日期",
            dataIndex: 'OFF_DATE',
            width: 90
        }/*, {
            text: "修改人員",
            dataIndex: 'UPDATE_BY',
            width: 100
        }, {
            text: "修改日期",
            dataIndex: 'UPDATE_DT',
            width: 150
        }*/]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        items: [T1Grid]
                    }
                ]
            }]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});
