Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T2GetExcel = '../../../api/BD0006/ExportExcel';

    var T1Set = '';
    var T1LastRec = null, T2LastRec = null;
    var T1Name = '廠商回覆狀態查詢';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];

    var viewModel = Ext.create('WEBAPP.store.BD0006VM');

    var T1Store = viewModel.getStore('MM_PO_M');
    var MATComboGet = '../../../api/BA0002/GetMATCombo';
    // 物品類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('P4').setValue(wh_nos[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setComboData();
    function T1Load() {
        T2Store.removeAll();
        T2LastRec = null;
        Ext.getCmp('btnExport').setDisabled(true);
        Ext.getCmp('btnSendMail').setDisabled(true);
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getRawValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getRawValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Tool.moveFirst();
    }

    var T2Store = viewModel.getStore('MM_PO_D');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["PO_TIME_N"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["PO_TIME_N"]);
            T2Store.getProxy().setExtraParam("p1", T1LastRec.data["ISBACK"]);
            T2Store.getProxy().setExtraParam("p4", T1LastRec.data["MAT_CLASS"]);
            T2Tool.moveFirst();
        }
    }

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'datefield',
                            fieldLabel: '訂單日期',
                            name: 'P0',
                            id: 'P0',
                            enforceMaxLength: true,
                            maxLength: 21,
                            labelWidth: 60,
                            width: 140,
                            padding: '0 4 0 4',
                            allowBlank: false,
                            fieldCls: 'required',
                            value: getDefaultValue(false),
                            regexText: '請選擇日期',
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            labelSeparator: '',
                            name: 'P1',
                            id: 'P1',
                            labelWidth: mLabelWidth,
                            width: 88,
                            labelWidth: 8,
                            padding: '0 4 0 4',
                            allowBlank: false,
                            fieldCls: 'required',
                            value: getDefaultValue(true),
                            regexText: '請選擇日期',
                        }, {
                            xtype: 'combo',
                            fieldLabel: '物料類別',
                            name: 'P4',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 170,
                            padding: '0 4 0 4',
                            store: MATQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        }, {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');

                                if (T1Query.getForm().isValid()) {
                                    T1Load();
                                } else {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>訂單日期</span>為必填');
                                }
                            }
                        }
                    ]
                }
            ]
        }]
    });
    function getDefaultValue(isEndDate) {
        var yyyy = new Date().getFullYear() - 1911;
        var m = new Date().getMonth() + 1;
        var d = 0;
        if (isEndDate) {
            d = new Date(yyyy, m, 0).getDate();
        } else {
            d = 1;
        }
        var mm = m >= 10 ? m.toString() : "0" + m.toString();
        var dd = d >= 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;

    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "訂單日期",
                dataIndex: 'PO_TIME_N',
                width: 120
            },
            {
                text: "收信回覆狀態",
                dataIndex: 'ISBACK',
                width: 100
            },
            {
                text: "收信數量",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'CNT',
                width: 80
            },
            {
                text: "送貨回覆數量",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'REPLY_CNT',
                width: 100
            },
            {
                text: "物料類別",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'MAT_CLASS',
                width: 80
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');
                T1LastRec = record;
                T2Load();
                Ext.getCmp('btnExport').setDisabled(false);
                Ext.getCmp('btnSendMail').setDisabled(true);
            }
        }
    });

    function T2GridSelectionCheck() {
        var selection = T2Grid.getSelection();
        if (selection.length) {
            Ext.getCmp('btnSendMail').setDisabled(false);
        }
        else {
            Ext.getCmp('btnSendMail').setDisabled(true);
        }
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '匯出',
                id: 'btnExport',
                name: 'btnExport',
                handler: function () {
                    var p = new Array();

                    if (T1LastRec != null && T1LastRec.data["PO_TIME_N"] !== '') {
                        p.push({ name: 'p0', value: T1LastRec.data["PO_TIME_N"] });
                        p.push({ name: 'p1', value: T1LastRec.data["ISBACK"] });
                        p.push({ name: 'p4', value: T1LastRec.data["MAT_CLASS"] });
                        PostForm(T2GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    }
                    else {
                        msglabel('訊息區:');
                    }
                }
            },
            {
                text: '重送MAIL',
                id: 'btnSendMail',
                name: 'btnSendMail',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let po_no = '';
                        //selection.map(item => {
                        //    po_no += item.get('PO_NO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            po_no += item.get('PO_NO') + ',';
                        })

                        Ext.MessageBox.confirm('重送MAIL', '要重送MAIL?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BD0006/SendEmail',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: po_no
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料更新成功');

                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            listeners: {
                selectionchange: function (model, record, index, eOpts) {
                    T2GridSelectionCheck();
                }
            }
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "收信回覆日期",
                style: 'text-align:left',
                dataIndex: 'ISBACK_DT',
                align: 'center',
                width: 120,
            },
            {
                text: "送貨回覆日期",
                style: 'text-align:left',
                align: 'center',
                dataIndex: 'REPLY_DT',
                width: 120
            },
            {
                text: "訂單編號",
                dataIndex: 'PO_NO',
                width: 120
            },
            {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 70
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 250,
                style: 'text-align:left'
            },
            {
                text: "廠商電話",
                dataIndex: 'AGEN_TEL',
                width: 120,
                style: 'text-align:left'
            },
            {
                text: "狀態",
                dataIndex: 'PO_STATUS',
                width: 100,
                style: 'text-align:left'
            },
            {
                text: "EMAIL",
                dataIndex: 'EMAIL',
                width: 160,
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {

                Ext.getCmp('btnExport').setDisabled(false);
                T2GridSelectionCheck();

                msglabel('訊息區:');

                T2LastRec = record;
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
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }
        ]
    });

    Ext.getCmp('btnExport').setDisabled(true);
    Ext.getCmp('btnSendMail').setDisabled(true);

});