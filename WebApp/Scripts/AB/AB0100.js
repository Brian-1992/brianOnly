Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var mLabelWidth = 80;
    var mWidth = 180;

    var viewModel = Ext.create('WEBAPP.store.AB0100VM');

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Store = viewModel.getStore('All');
    function T1Load(clearMsg) {

        if (clearMsg) {
            msglabel('');
        }

        T1Store.getProxy().setExtraParam("apptime_s", T1Query.getForm().findField('P0').rawValue);
        T1Store.getProxy().setExtraParam("apptime_e", T1Query.getForm().findField('P1').rawValue);
        T1Tool.moveFirst();
    }

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
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    width: '100%',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'datefield',
                            fieldLabel: '申請日期',
                            name: 'P0',
                            id: 'P0',
                            fieldCls:'required',
                            labelWidth: mLabelWidth,
                            width: 180,
                            allowBlank: true,
                            padding: '0 4 0 4',
                            value: new Date(new Date().getFullYear(), new Date().getMonth(), 1)
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            name: 'P1',
                            id: 'P1',
                            fieldCls: 'required',
                            labelWidth: 8,
                            labelSeparator: '',
                            width: 98,
                            allowBlank: true,
                            padding: '0 4 0 4',
                            value: new Date()
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            id:'btnSearch',
                            handler: function () {
                                msglabel('訊息區:');
                                var f = T1Query.getForm();
                                if (!f.findField('P0').getValue() || !f.findField('P1').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>申請日期</span>為必填');
                                    return;
                                }

                                T1Load(true);
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            id: 'btnClear',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        },
                        {
                            xtype: 'button',
                            text: '轉入申請資料',
                            id: 'btnTransfer',
                            handler: function () {

                                Ext.Ajax.timeout = 300000;
                                Ext.Ajax.setTimeout(300000);
                                Ext.override(Ext.data.proxy.Server, { timeout: Ext.Ajax.timeout });
                                Ext.override(Ext.data.Connection, { timeout: Ext.Ajax.timeout });

                                Ext.getCmp('btnSearch').disable();
                                Ext.getCmp('btnClear').disable();
                                Ext.getCmp('btnTransfer').disable();

                                Ext.Ajax.request({
                                    url: '/api/AB0100/Transfer',
                                    method: reqVal_p,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('資料轉入成功');
                                            T1Load(false);

                                            Ext.getCmp('btnSearch').enable();
                                            Ext.getCmp('btnClear').enable();
                                            Ext.getCmp('btnTransfer').enable();
                                        }
                                    },
                                    failure: function (response, options) {

                                    }
                                });
                            }
                        }
                    ]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true,
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
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
                text: "請領單號",
                dataIndex: 'PURCHNO',
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "申請數量",
                dataIndex: 'APPQTY',
                width: 100
            },
            {
                text: "計量單位代碼",
                dataIndex: 'BASE_UNIT',
                width: 100
            },
            {
                text: "申請人員",
                dataIndex: 'APPUSR',
                width: 100
            },
            {
                text: "申請時間",
                dataIndex: 'APPTIME',
                width: 100
            },
            {
                text: "申請單備註",
                dataIndex: 'APPLY_NOTE',
                width: 100
            },
            {
                text: "新增時間",
                dataIndex: 'INSTIME',
                width: 100
            },
            {
                text: "讀取時間",
                dataIndex: 'RDTIME',
                width: 100
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "不符原因",
                dataIndex: 'REJ_NOTE',
                width: 100
            },
        ],
        
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

});