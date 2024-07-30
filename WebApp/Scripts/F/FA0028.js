﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1LastRec = null;

    var viewModel = Ext.create('WEBAPP.store.FA0028VM');
    var T1Store = viewModel.getStore('MI_WLOCINV');
    T1Store.on({
        load: function (store, options) {   //設定匯入,列印是否disable
            var dataCount = store.getCount();
            if (dataCount > 0) {
                Ext.getCmp('export').setDisabled(false);
                Ext.getCmp('print').setDisabled(false);
            } else {
                Ext.getCmp('export').setDisabled(true);
                Ext.getCmp('print').setDisabled(true);
            }
        }
    });
    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        //T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        //T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        //T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        //T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());

        //T1Store.getProxy().setExtraParam("TASKID", userTaskId);
        //T1Store.getProxy().setExtraParam("APPDEPT", userInid);
        T1Tool.moveFirst();
        //T1Store.loadPage(1);
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
    }

    var MatClassStore = viewModel.getStore('MAT_CLASS');
    function MatClassLoad() {
        MatClassStore.load({
            params: {
                start: 0
            }
        });
    }

    var WhNoStore = viewModel.getStore('WH_NO');
    function WhNoLoad() {
        WhNoStore.load({
            params: {
                start: 0
            }
        });
    }

    //院內碼
    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 80,
        width: 250,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/FA0028/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });


    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
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
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                //bodyStyle: 'padding: 3px 5px;',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '庫房代碼',
                        store: WhNoStore,
                        name: 'P0',
                        id: 'P0',
                        labelWidth: 80,
                        width: 250,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
                    },
                    {
                        xtype: 'combo',
                        fieldLabel: '物料分類',
                        store: MatClassStore,
                        name: 'P1',
                        id: 'P1',
                        labelWidth: 80,
                        width: 250,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE'
                    },
                    T1QuryMMCode,
                    {
                        xtype: 'button',
                        text: '查詢',
                        style: 'margin:0px 5px 0px 20px;',
                        handler: function () {
                            T1Load();
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        style: 'margin:0px 5px;',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
                    }
                ]
            },
        ]
    });


    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export',
                text: '匯出',
                id: 'export',
                name: 'export',
                handler: function () {
                   
                    var p0 = T1Query.getForm().findField('P0').getValue();
                    var p1 = T1Query.getForm().findField('P1').getValue();
                    var p1_Name = T1Query.getForm().findField('P1').getRawValue();
                    p1_Name = Ext.util.Format.substr(p1_Name, 3, p1_Name.length);
                    var p2 = T1Query.getForm().findField('P2').getValue();
                    var p = new Array();

                    p.push({ name: 'P0', value: p0 });
                    p.push({ name: 'P1', value: p1 });
                    p.push({ name: 'P1_Name', value: p1_Name });
                    p.push({ name: 'P2', value: p2 });
                    //alert(p[1].value);
                    PostForm('../../../api/FA0028/Excel', p);
                }
            },
            {
                itemId: 'print',
                id: 'print',
                name: 'print',
                text: '列印',
                style: 'margin:0px 5px;',
                handler: function () {
                    showReport();
                }
            }
        ]
    });

    function showReport() {
        if (!win) {
            //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料分類名稱)
            var tmp_p1_Name = T1Query.getForm().findField('P1').getRawValue();
            tmp_p1_Name = Ext.util.Format.substr(tmp_p1_Name, 3, tmp_p1_Name.length);

            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1_Name: tmp_p1_Name,
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T1Query.getForm().findField('P2').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="/Report/F/FA0028.aspx?p0=' + np.p0 + '&p1_Name=' + tmp_p1_Name + '&p1=' + np.p1 + '&p2=' + np.p2 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        },
        {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        },{
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            style: 'text-align:left',
            align: 'left',
            width: 300
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 300
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            style: 'text-align:left',
            align: 'left',
            width: 65
        }, {
            text: "儲位碼",
            dataIndex: 'STORE_LOC',
            style: 'text-align:left',
            align: 'left',
            width: 65
        }
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
        },
        ]
    });
    Ext.getCmp('export').setDisabled(true);
    Ext.getCmp('print').setDisabled(true);
});

