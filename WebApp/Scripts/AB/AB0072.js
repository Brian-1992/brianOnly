Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var St_MatclassGet = '../../../api/AA0061/GetMatclassCombo';
    var T1GetExcel = '../../../api/AA0061/ExcelByDept';


    var reportUrl = '/Report/A/AA0061.aspx';　　　　　　　　//明細

    // 物料群組
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    function setComboData() {

        //物料分類
        Ext.Ajax.request({
            url: St_MatclassGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    //var tb_matclass = data.etts;
                    //T1Query.getForm().findField('SHOWOPT').setValue({ SHOWOPTRB: 1 });   // 初始化RB
                    //T1Query.getForm().findField('SHOWDATA').setValue({ SHOWDATARB: 1 }); // 初始化RB


                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].COMBITEM });
                        }

                        if (tb_matclass.length == 1) {
                            //1筆資料時將
                            T1Query.getForm().findField('P0').setValue(tb_matclass[0].VALUE);
                        }
                        else {
                            T1Query.getForm().findField('P0').setDisabled(false);
                        }
                    }

                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '藥品代碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 280,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0061/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: '01', 
                store_id: T1Query.getForm().findField('SHOWOPT').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    // 查詢欄位
    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    function get6monthday() {
        var rtnDay = addMonths(new Date(), -6);
        return rtnDay
    }
    function get1monthday() {
        var rtnDay = addMonths(new Date(), -1);
        return rtnDay
    }
    function addMonths(date, months) {
        date.setMonth(date.getMonth() + months);
        return date;
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
                    fieldLabel: 'Update',
                    name: 'x',
                    xtype: 'hidden'
                }, {
                    xtype: 'radiogroup',
                    anchor: '40%',
                    labelWidth: 25,
                    width: 110,
                    name: 'SHOWOPT',
                    items: [
                        { boxLabel: '月報', width: 50, name: 'SHOWOPTRB', inputValue: 1 },
                        { boxLabel: '季報', width: 50, name: 'SHOWOPTRB', inputValue: 0 },
                        { boxLabel: '年報', width: 50, name: 'SHOWOPTRB', inputValue: 2 }
                    ],
                    listeners:
                    {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                            Ext.Msg.alert('說明', T1Query.getForm().findField('P0').getValue());
                        },
                        change: function (rg, nVal, oVal, eOpts) {   //mat_class 物料分類的值, nVal['SHOWOPT'] 庫備識別碼(0非庫備, 1庫備)

                        }
                    }
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '查詢日期',
                    labelAlign: 'right',
                    name: 'P0',
                    id: 'P0',
                    vtype: 'dateRange',
                    dateRange: { end: 'P1' },
                    padding: '0 4 0 4',
                    labelWidth: 90,
                    width: 230
                    , value: get6monthday()
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelAlign: 'right',
                    labelWidth: 10,
                    name: 'P1',
                    id: 'P1',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P0' },
                    padding: '0 4 0 4',
                    width: 150
                    , value: getToday()
                }, 
                T1QuryMMCode, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load,
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        T1Query.getForm().findField('P0').reset();
                        T1Query.getForm().findField('P1').reset();

                        var f = this.up('form').getForm();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]

    });

    var T1Store = Ext.create('WEBAPP.store.ItemAllocationByDept', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    showopt: T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue,
                    showdata: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue,
                };
                Ext.apply(store.proxy.extraParams, np);
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
                id: 'export',
                name: 'export',
                text: '匯出', //T1Query
                handler: function () {
                    

                }
            }

        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?type=' + "AB0056"
                    + '&APPTIME1=' + T1Query.getForm().findField('D0').getValue()
                    + '&APPTIME2=' + T1Query.getForm().findField('D1').getValue()
                    + '&task_id=' + T1Query.getForm().findField('P0').getValue()
                    + '&mmcode=' + T1Query.getForm().findField('P1').getValue()
                    + '&showopt=' + T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue
                    + '&showdata=' + T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue
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
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "藥品代碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "藥品名稱",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 150
            },{
                text: "庫別",
                dataIndex: 'INID_NAME_USER',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "上月結",
                dataIndex: 'APVQTYN',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "本月進",
                dataIndex: 'AVG_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "本月出",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "現品量",
                dataIndex: 'M_ALLPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "調整量",
                dataIndex: 'APVQTYN',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "安全量",
                dataIndex: 'AVG_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "前一月消耗量",
                dataIndex: 'APVQTYN',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "前二月消耗量",
                dataIndex: 'APVQTYN',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "前三月消耗量",
                dataIndex: 'APVQTYN',
                style: 'text-align:left',
                align: 'right',
                width: 100
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

    T1Query.getForm().findField('D0').focus();

});