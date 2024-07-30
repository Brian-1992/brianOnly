Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
 

    var T1Rec = 0;
    var T1LastRec = null;
    
    //var d2;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 物料分類清單
   

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
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                   xtype: 'datefield',
                    id: 'P1',
                    name: 'P1',
                    fieldCls:'required',
                    fieldLabel: '送申請日期',
                    width: 180,
                    labelWidth: 90,
                    listeners: {
                        change: function (field, nVal, oVal, eOpts) {
                            T1Query.getForm().findField('P2').setMinValue(nVal);
                        }
                    }
                }, {
                    xtype: 'datefield',
                    id: 'P2',
                    name: 'P2',
                    fieldLabel: '至',
                    width: 110,
                    fieldCls: 'required',
                    labelWidth: 20,
                    listeners: {
                        change: function (field, nVal, oVal, eOpts) {
                            T1Query.getForm().findField('P1').setMaxValue(nVal);
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {

                        if (!T1Query.getForm().findField('P2').getValue() ||
                            !T1Query.getForm().findField('P1').getValue()) {
                            Ext.Msg.alert('提醒', '送申請日期為必填');
                            return;
                        }

                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_E', 'WH_NAME', 'AGEN_NAMEE', 'APVQTY', 'TOWH', 'DOCNO']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });


    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'AGEN_NAMEE', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                   
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
            url: '/api/AA0077/GetALL',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '匯出', border: 1,
                style: {
                    borderColor: '#0080ff',
                    borderStyle: 'solid'
                },
                handler: function () {

                    //Ext.Ajax.request({
                    //    url: '/api/AA0077/GetExcel',
                       
                    //    params: {
                    //        p1: T1Query.getForm().findField('P1').getValue(),
                    //        p2: T1Query.getForm().findField('P2').getValue(),
                    //    },
                    //    success: function () {
                           
                    //    },
                    //    failure: function () {
                           
                    //    }
                    //});
                    
                    var d1 = T1Query.getForm().findField('P1').rawValue;
                    var d2 = T1Query.getForm().findField('P2').rawValue;
                    var dbg = (parseInt(d1.substring(0, 3)) + 1911); 
                    var dbg2 = d1.substring(3, 7)
                    var ded = (parseInt(d2.substring(0, 3)) + 1911);
                    var ded2 = d2.substring(3, 7)
                    ded = ded + ded2;
                    dbg = dbg + dbg2;
                    var e = dbg + ',' + ded + ',' + T1Query.getForm().findField('P1').rawValue + ',' + T1Query.getForm().findField('P2').rawValue;
                   
                    var p = new Array();
                    p.push({ name: 'FN', value: '大瓶點滴之申請量總表.xls' }); //檔名
                    p.push({ name: 'TS', value: e }); //SQL篩選條件
                    PostForm('/api/AA0077/GetExcel', p);
                }
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 350
        }, {
            text: "廠商英文名稱",
            dataIndex: 'AGEN_NAMEE',
            width: 150
        }, {
            text: "入庫庫房代碼",
            dataIndex: 'TOWH',
            width: 110
           
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 120
        }, {
            text: "核發數量",
            dataIndex: 'APVQTY',
            align: 'right', 
            style:'text-align:left',
            width: 80
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
             width: 150
        }, {
            header: "",
            flex: 1
        }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ]
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
        }]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    
});