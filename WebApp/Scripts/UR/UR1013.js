Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.File']);

Ext.onReady(function () {
    Ext.ariaWarn = Ext.emptyFn;
    var T1Get = '/api/UR1012/Show';
    //var T1Name = "系統公佈欄";
    var T1Name = '';

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var T1F1 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['ID', 'TITLE', 'CONTENT',
            { name: 'ON_DATE', type: 'date' },
            { name: 'OFF_DATE', type: 'date' },
            'CREATE_BY',
            { name: 'CREATE_DT', type: 'date' },
            { name: 'ATTACHMENTS', type: 'auto' }]
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
        T1Store.loadPage(1);
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

    //懸浮提示初始化
    Ext.QuickTips.init();
    //屬性
    Ext.apply(Ext.QuickTips.getQuickTip(), {
        maxWidth: 700,
        dismissDelay: 60000,
        trackMouse: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //viewConfig: {
        //    getRowClass: function (record, rowIndex, rowParams, store) { return 'multiline-row'; }
        //},
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
        columns: [
        { xtype: 'rownumberer' },
        {
            text: "標題",
            dataIndex: 'TITLE',
            width: 200,
            draggable: false,
            renderer: function (value, metaData, record, rowIndex) {
                if (value) {
                    value = Ext.String.htmlEncode(value);
                    metaData.tdAttr = 'data-qtip="' + Ext.String.htmlEncode(value) + '"';
                }
                
                if (record.data.IS_TODAY == 'Y') {
                    return '<span style="color:red; font-size:20px;">' + value + '</span>';
                } else {
                    return '<span style="font-size:20px;">' + value + '</span>';
                }
            }
        }, {
            text: "內容",
            dataIndex: 'CONTENT',
            width: 550,
            draggable: false,
            renderer: function (value, metaData, record, rowIndex) {
                if (value) {
                    value = Ext.String.htmlEncode(value);
                    metaData.tdAttr = 'data-qtip="' + Ext.String.htmlEncode(value.replace(/\n/g, '<br/>')) + '"';
                }
                
                if (record.data.IS_TODAY == 'Y') {
                    return '<span style="color:red; font-size:20px;">' + value + '</span>';
                } else {
                    return '<span style="font-size:20px;">' + value + '</span>';
                }
            }
        },
        {
            text: "附件",
            width: 180,
            //align: 'center',
            draggable: false,
            renderer: function (val, metaData, record) {
                var attachments = record.get('ATTACHMENTS');
                var result = '';
                var fileNames = '';
                Ext.each(attachments, function (obj, index, value) {
                    if (result != '') result += '<br/>';
                    result += '<a style="font-size:20px;" href=\'javascript:FileUtil.DownloadFile({fileGuid:\"' + obj['FG'] + '\"});\'><img src="../../../Images/TRA/save.gif" align="absmiddle" /> ' + obj['FN'] + '</a>';
                    fileNames += obj['FN'] + '?';
                });
                if (fileNames) {
                    value = Ext.String.htmlEncode(fileNames);
                    metaData.tdAttr = 'data-qtip="' + Ext.String.htmlEncode(value.replace('?', '<br/>')) + '"';
                }

                return result;
                /*
                var ac = parseInt(record.get("ATTACH_CNT"));
                if (ac > 0)
                    return '<a href=\'javascript:showUploadWindowL(\"' + record.get("ID") + '\", \"' + record.get("TITLE") +'\");\'><img src="../../../Images/TRA/save.gif" align="absmiddle" /></a>';
                //return '<a href=javascript:getFileName("' + record.get('FG') + '");>' + val + '</a>';
                return "";
                */
            }
        }, {
            text: "公告人員",
            dataIndex: 'CREATE_BY',
            width: 100,
            draggable: false,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.IS_TODAY == 'Y') {
                    return '<span style="color:red; font-size:20px;">' + value + '</span>';
                } else {
                    return '<span style="font-size:20px;">' + value + '</span>';
                }
            }
        }/*, {
            text: "公告時間",
            dataIndex: 'CREATE_DT',
            width: 150
        }*/, {
            xtype: 'datecolumn',
            text: "起始日期",
            dataIndex: 'ON_DATE',
            format: 'X/m/d',
            width: 130,
            draggable: false,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.IS_TODAY == 'Y') {
                    return '<span style="color:red; font-size:20px;">' + Ext.util.Format.date(value, 'X/m/d') + '</span>';
                } else {
                    return '<span style="font-size:20px;">' + Ext.util.Format.date(value, 'X/m/d') + '</span>';
                }
            }
        }, {
            xtype: 'datecolumn',
            text: "結束日期",
            dataIndex: 'OFF_DATE',
            format: 'X/m/d',
            width: 130,
            flex: 1,
            draggable: false,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.IS_TODAY == 'Y') {
                    return '<span style="color:red; font-size:20px;">' + Ext.util.Format.date(value, 'X/m/d') + '</span>';
                } else {
                    return '<span style="font-size:20px;">' + Ext.util.Format.date(value, 'X/m/d') + '</span>';
                }
            }
        }/*, {
            text: "修改人員",
            dataIndex: 'UPDATE_BY',
            width: 100
        }, {
            text: "修改日期",
            dataIndex: 'UPDATE_DT',
            width: 150
        }*/],
        listeners: {
            cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {
                if (cellIndex == 2) {
                    var content = record.data['CONTENT'];
                    if (content)
                        Ext.Msg.alert('<span style="font-size:16px;">' + record.data['TITLE'] + '</span>',
                            '<span style="font-size:18px;">' + content.replace(/\n/g, '<br/>') + '</span>');
                }
            }
        }
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

    //T1Query.getForm().findField('P0').focus();
});
