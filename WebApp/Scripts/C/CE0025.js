Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Get = '/api/CE0025/GetQueryData';
    var reportUrl = '/Report/C/CE0025.aspx';
    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'MMCODE',
            'MMNAME_C',
            'MMNAME_E',
            'BASE_UNIT',
            'STORE_QTY',
            'CHK_QTY',
            'GAP_T',
            'MEMO'
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        //autoLoad: true,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    chkYM: T1Query.getForm().findField('CHK_YM').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
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
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                + '?chkYM=' + T1Query.getForm().findField('CHK_YM').rawValue
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    function getChkPeriod() {
        Ext.Ajax.request({
            url: '/api/CE0027/GetChkPeriod',
            method: reqVal_p,
            params: { chk_ym: T1Query.getForm().findField('CHK_YM').rawValue },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('P2').setValue(data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getAploutqtyDateRange() {
        Ext.Ajax.request({
            url: '/api/CE0024/AploutqtyDateRange',
            method: reqVal_p,
            params: { chk_ym: T1Query.getForm().findField('CHK_YM').rawValue },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('P3').setValue('<span style="color:blue">' + data.msg + '</span>');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'monthfield',
                    fieldLabel: '盤點年月',
                    name: 'CHK_YM',
                    id: 'CHK_YM',
                    width: 130,
                    padding: '0 10 0 0',
                    fieldCls: 'required',
                    value: new Date()
                }, {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {                        
                        if (T1Query.getForm().findField('CHK_YM').rawValue === null) {
                            Ext.Msg.alert('訊息', "請點選盤點年月");
                        }
                        else {
                           T1Load();
                            getChkPeriod();
                            getAploutqtyDateRange();
                        }

                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();

                        f.reset();

                        f.findField('CHK_YM').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '本次盤點期',
                    name: 'P2',
                    id: 'P2',
                    value: '',
                    labelWidth: 70,
                    width: 130
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '',
                    name: 'P3',
                    id: 'P3',
                    value: '',
                    width: 230
                },
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '列印品項差異表', handler: function () {
                    if (T1Store.getCount() > 0) {
                        showReport();
                    }
                    else {
                        Ext.Msg.alert('訊息', '無資料可列');
                    }
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "電腦量",
                dataIndex: 'STORE_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "差異量",
                dataIndex: 'GAP_T',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "備註",
                dataIndex: 'MEMO',
                width: 200
            },
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
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
            items: [T1Grid]
        }
        ]
    });

    T1Load();
    getChkPeriod();
    getAploutqtyDateRange();
});