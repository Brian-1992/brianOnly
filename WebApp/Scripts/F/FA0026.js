﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var reportUrl = '/Report/F/FA0026.aspx';

    var T1RecLength = 0;
    var T1LastRec = null;

    var T1GetExcel = '../../../api/FA0026/Excel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //物料分類Store
    var st_MatClass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0026/GetMatClassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    //院內碼
    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 80,
        width: 250,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/FA0026/GetMMCODECombo', //指定查詢的Controller路徑
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
        layout: 'hbox',
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
                bodyStyle: 'padding: 3px 5px;',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '物料分類',
                        store: st_MatClass,
                        name: 'P0',
                        id: 'P0',
                        labelWidth: 80,
                        width: 250,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE'
                    },
                    T1QuryMMCode
                ]
            },
            {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                bodyStyle: 'padding: 3px 5px;',
                items: [

                    {
                        xtype: 'radiogroup',
                        anchor: '40%',
                        labelWidth: 20,
                        width: 160,
                        name: 'P2',
                        id: 'P2',
                        items: [
                            {
                                boxLabel: '庫備',
                                width: 70,
                                name: 'P2',
                                inputValue: 1,
                                checked: true
                            },
                            {
                                boxLabel: '非庫備',
                                width: 70,
                                name: 'P2',
                                inputValue: 0
                            }
                        ],
                        padding: '0 4 0 15'
                    }, {
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
            }]
    });

    Ext.define('FA0026_Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'UPRICE', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'EXCH_RATIO', type: 'string' },
            { name: 'M_AGENNO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_MATID', type: 'string' },
            { name: 'DISC_UPRICE', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' },
            { name: 'M_APPLYID', type: 'string' },
            { name: 'M_VOLL', type: 'string' },
            { name: 'M_VOLW', type: 'string' },
            { name: 'M_VOLH', type: 'string' },
            { name: 'M_VOLC', type: 'string' },
            { name: 'M_SWAP', type: 'string' },
            { name: 'M_PHCTNCO', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'FA0026_Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0026/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('export').setDisabled(false);
                    //Ext.getCmp('print').setDisabled(false);
                } else {
                    Ext.getCmp('export').setDisabled(true);
                    //Ext.getCmp('print').setDisabled(true);
                }
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
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
                    //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
                    var p0_Name = T1Query.getForm().findField('P0').getRawValue();
                    p0_Name = Ext.util.Format.substr(p0_Name, 3, p0_Name.length);
                    var p1 = T1Query.getForm().findField('P1').getValue();
                    var p2 = T1Query.getForm().findField('P2').getValue().P2;
                    var p = new Array();

                    p.push({ name: 'P0', value: p0 });
                    p.push({ name: 'P0_Name', value: p0_Name });
                    p.push({ name: 'P1', value: p1 });
                    p.push({ name: 'P2', value: p2 });
                    PostForm(T1GetExcel, p);
                }
            },
            //{
            //    itemId: 'print',
            //    id: 'print',
            //    name: 'print',
            //    text: '列印',
            //    style: 'margin:0px 5px;',
            //    handler: function () {
            //        showReport();
            //    }
            //}
        ]
    });

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
        columns: [
            {
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
            width: 210
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 180
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            style: 'text-align:left',
            align: 'left',
            width: 65
        }, {
            text: "包裝單位",
            dataIndex: 'M_PURUN',
            style: 'text-align:left',
            align: 'left',
            width: 65
        }, {
            text: "進貨單價",
            dataIndex: 'UPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 90
        }, {
            text: "合約價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 90
        }, {
            text: "申購轉換率",
            dataIndex: 'EXCH_RATIO',
            style: 'text-align:left',
            align: 'right',
            width: 85
        }, {
            text: "廠編",
            dataIndex: 'M_AGENNO',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "是否庫備",
            dataIndex: 'M_STOREID',
            style: 'text-align:left',
            align: 'left',
            width: 70
        }, {
            text: "是否合約",
            dataIndex: 'M_CONTID',
            style: 'text-align:left',
            align: 'left',
            width: 70
        }, {
            text: "是否聯標",
            dataIndex: 'M_MATID',
            style: 'text-align:left',
            align: 'left',
            width: 85
        }, {
            text: "優惠後進貨單價",
            dataIndex: 'DISC_UPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 110
        }, {
            text: "優惠後合約價",
            dataIndex: 'DISC_CPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 110
        }, {
            text: "特殊識別碼",
            dataIndex: 'M_APPLYID',
            style: 'text-align:left',
            align: 'left',
            width: 90
        }, {
            text: "長",
            dataIndex: 'M_VOLL',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "寬",
            dataIndex: 'M_VOLW',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "高",
            dataIndex: 'M_VOLH',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "圓周",
            dataIndex: 'M_VOLC',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "材積轉換率",
            dataIndex: 'M_SWAP',
            style: 'text-align:left',
            align: 'right',
            width: 85
        }, {
            text: "環保或衛署證號",
            dataIndex: 'M_PHCTNCO',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
    });

    function showReport() {
        if (!win) {
            //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
            var tmp_p0_Name = T1Query.getForm().findField('P0').getRawValue();
            tmp_p0_Name = Ext.util.Format.substr(tmp_p0_Name, 3, tmp_p0_Name.length);

            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p0_Name: tmp_p0_Name,
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T1Query.getForm().findField('P2').getValue().P2
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p0_Name=' + tmp_p0_Name + '&p1=' + np.p1 + '&p2=' + np.p2 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
    //Ext.getCmp('print').setDisabled(true);
});