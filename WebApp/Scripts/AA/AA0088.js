Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    //var T1Name = "藥局進貨作業";
    var T1Rec = 0;
    var T1LastRec = null;
    //var IsPageLoad = true;
    // var pSize = 100; //分頁時每頁筆數
    var reportUrl = '/Report/A/AA0088.aspx';

  

    var mLabelWidth = 80;
    var mWidth = 170;
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
                    xtype: 'textfield',
                    id: 'P1',
                    name: 'P1',
                    fieldLabel: '查詢日期起',
                    width: 170,
                    enforceMaxLength: true, //最多輸入的長度有限制
                    maxLength: 7,          //最多輸入10碼
                    maskRe: /[0-9]/,      //前端只能輸入數字跟斜線
                    regexText: '正確格式應為「YYYMMDD」，<br>例如「1080131」',
                    regex: /^[0~9]{7}$/,
                }, {
                    xtype: 'textfield',
                    id: 'P2',
                    name: 'P2',
                    fieldLabel: '訖',
                    width: 170,
                    enforceMaxLength: true,
                    maxLength: 7,          //最多輸入10碼
                    maskRe: /[0-9]/,      //前端只能輸入數字跟斜線
                    regexText: '正確格式應為「YYYMMDD」，<br>例如「1080331」',
                    regex: /^[0~9]{7}$/,
               
               
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_E', 'CONTRACNO', 'E_COMPUNIT', 'E_MANUFACT', 'E_SUPSTATUS', 'INV_QTY', 'BASE_UNIT', 'M_CONTPRICE', 'APL_OUTQTY', 'CNT_QTY', 'ADJ_QTY', 'TOTAL', 'E_SCIENTIFICNAME']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    // p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                  
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0088/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });



    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
               
                itemId: 't1print', text: '列印', disabled: false, handler: function () {
                    showReport();
                }
            }
        ]
    });



    //Ext.tip.QuickTipManager.init();

    var T1Grid = Ext.create('Ext.grid.Panel', {
        menuDisabled: true,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [
        //    Ext.create("Ext.grid.plugin.CellEditing", {
        //        //clicksToEdit: 1,
        //    })
        //],
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    layout: 'fit',
            //    items: [T1Query]
            //},
            {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: '聯標合約項次', dataIndex: 'CONTRACNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '院內碼', dataIndex: 'MMCODE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '商品名', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '成分名', dataIndex: 'E_SCIENTIFICNAME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '廠牌', dataIndex: 'E_MANUFACT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '單位', dataIndex: 'BASE_UNIT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '單價', dataIndex: 'M_CONTPRICE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '屯儲數量', dataIndex: 'INV_QTY', align: 'right', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '金額', dataIndex: 'TOTAL', align: 'right', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '廠商', dataIndex: 'AGEN_NAME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '規定屯量', dataIndex: '', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '備考', dataIndex: '', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            
        ],

    });



    function T1Load() {
        T1Tool.moveFirst();
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    function T2Load() {
        T2Store.load({
            params: {
                p0: Dno,
            }
        });
    }

    function showReport() {
        var np = {
            p0: T1Query.getForm().findField('P1').getValue(),
            p1: T1Query.getForm().findField('P2').getValue(),
        };
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                //html: '<iframe src="' + reportUrl + '?WH_NO=' + WH_NO + '&STORE_LOC=' + STORE_LOC + '&BARCODE_IsUsing=' + BARCODE_IsUsing + '&STATUS=' + STATUS + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',

                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        }]
    });

    T1Load();
});