Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});




var T1GetExcel = '/api/AA0108/Excel';
var reportUrl = '/Report/C/CE0026.aspx';
var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';
var P4 = '';
var P5 = '';
var P6 = '';
var P7 = '';

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    function getChkPeriod() {
        Ext.Ajax.request({
            url: '/api/CE0027/GetChkPeriod',
            method: reqVal_p,
            params: { chk_ym: T1Query.getForm().findField('P0').rawValue },
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
            params: { chk_ym: T1Query.getForm().findField('P0').rawValue },
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

    // 查詢欄位
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
            border: false,
            layout: 'hbox',
            items: [
                {
                 
                    xtype: 'monthfield',
                    fieldLabel: '月份別',
                    name: 'P0',
                    id: 'P0',
                    width: 160,
                    labelWidth: 60,
                    fieldCls: 'required',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    padding: '0 4 0 4',
                    //fieldCls: 'required',
                    allowBlank: false,
                    value: new Date()
                }, {


                    xtype: 'radiogroup',
                    fieldLabel: '顯示內容',
                    id: 'rd2',
                    width: 400,
                    hidden: false,
                    labelWidth: 60,
                    items: [

                        { boxLabel: '全部', name: 'rb1', itemid: 'r1', width: 70, inputValue: '0', checked: true },
                        { boxLabel: '盤盈', name: 'rb1', width: 70, inputValue: '1' },
                        { boxLabel: '盤虧', name: 'rb1', width: 70, inputValue: '2' }
                    ]
               
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            P0 = f.findField('P0').rawValue;
                            P1 =f.findField('rb1').getGroupValue();

                            T1Load();
                            getChkPeriod();
                            getAploutqtyDateRange();
                        }
                        else {
                            Ext.MessageBox.alert('提示', '請輸入必填欄位');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
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
    
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'STORE_QTY', 'STORE_COST', 'CHK_QTY', 'CHK_COST', 'diff_P', 'DIFF_COST']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

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
                  
                    P0: P0,
                    P1: P1,
                
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            timeout: 90000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0026/GetAll',
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
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1print', text: '列印', disabled: false,
                handler: function () {
                    showReport();
                }
          
            }
        ]
    });

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').rawValue,
                p1: T1Query.getForm().findField('rb1').getGroupValue(),
             
            };
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
        columns: [
            {
                xtype: 'rownumberer'
           
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 200
            }, {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 200
            }, {
                text: "庫存量",
                dataIndex: 'STORE_QTY',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "庫存成本",
                dataIndex: 'STORE_COST',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "盤點數量",
                dataIndex: 'CHK_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "實盤成本",
                dataIndex: 'CHK_COST',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "差異百分比",
                dataIndex: 'diff_P',
                align: 'right',
                width: 80
            }, {
                text: "差異成本  ",
                dataIndex: 'DIFF_COST',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "備註",
                dataIndex: 'MEMO',
                width: 200
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                   // T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#t1print').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
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

    var d = new Date();
    m = d.getMonth(); //current month
    y = d.getFullYear(); //current year

    P0 = T1Query.getForm().findField('P0').rawValue;
    P1 = T1Query.getForm().findField('rb1').getGroupValue();
    T1Load();
    getChkPeriod();
    getAploutqtyDateRange();
    //T1Query.getForm().findField('P1').setValue(new Date(y, m - 1));
    //T1Query.getForm().findField('P2').setValue(new Date(y, m));

});