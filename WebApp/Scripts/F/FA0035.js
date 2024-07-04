Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

function SetDate() {
    nowDate = new Date();
    nowDate.getMonth();
    nowDate = Ext.Date.format(nowDate, "Ymd") - 19110000;
    nowDate = nowDate.toString().substring(0, 5);
   
    T1Query.getForm().findField('P1').setValue(nowDate);
    
}

var MATComboGet = '../../../api/FA0035/GetMATCombo';
var matUserID;
var MMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
    id: 'P3',
    name: 'P3',
    fieldLabel: '院內碼',
    labelAlign: 'right',
    labelWidth: 60,
    width: 180,
    //fieldCls: 'required',
    //allowBlank: false,
                    //value: date_e
    limit: 10, //限制一次最多顯示10筆
    queryUrl: '/api/AA0061/GetMMCODECombo', //指定查詢的Controller路徑
    extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
    getDefaultParams: function () { //查詢時Controller固定會收到的參數
        return {
            
        };
    },
    listeners: {
        select: function (c, r, i, e) {
            //選取下拉項目時，顯示回傳值
        }
    },
});

// 物品類別清單
var MATQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});


function setComboData() {
    Ext.Ajax.request({
        url: MATComboGet,
        params: {
            p0: matUserID
        },
        method: reqVal_g,
        success: function (response) {
            var data = Ext.decode(response.responseText);
            if (data.success) {
                var wh_nos = data.etts;
                if (wh_nos.length > 0) {
                    for (var i = 0; i < wh_nos.length; i++) {
                        MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                    }
                }
            }
        },
        failure: function (response, options) {

        }
    });
}
setComboData();


var T1GetExcel = '/api/FA0035/Excel';
var reportUrl = '/Report/F/FA0035.aspx';
var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';
var P4 = '';
var P5 = '';
var P6 = '';
var P7 = '';
var from_y = '';
//  alert(from_y)
var to_y = '';
varfrom_m = '';
// alert(from_m)
varto_m = '';
//  alert(to_m)
varv_ym = '';

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

   

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
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P0',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    padding: '0 4 0 4',
                    store: MATQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '月份別',
                    name: 'P1',
                    id: 'P1',
                    width: 170,
                    labelWidth: 70,
                    //fieldCls: 'required',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    padding: '0 4 0 4',
                    //fieldCls: 'required',
                    // allowBlank: false
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'P2',
                    labelWidth: 8,
                    width: 88,
                    padding: '0 4 0 4',
                    labelSeparator: '',
                    //fieldCls: 'required',
                    //allowBlank: false,
                    //value: date_e

                }, {


                    xtype: 'radiogroup',
                    fieldLabel: '庫備類別',
                    width: 300,
                    labelWidth: 80,
                    items: [
                        { boxLabel: '所有品項', name: 'rb1', width: 70, inputValue: '2', checked: true },
                        { boxLabel: '庫備品', name: 'rb1', width: 70, inputValue: '1' },
                        { boxLabel: '非庫備品', name: 'rb1', width: 70, inputValue: '0', }
                    ]
                }, MMCode

                , {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            P0 = f.findField('P0').getValue();
                            P1 = f.findField('P1').rawValue;
                            P2 = f.findField('P2').rawValue;
                            P3 = f.findField('P3').getValue();
                            P4 = f.findField('rb1').getGroupValue();
                            P5 = f.findField('P1').rawValue;
                            from_y = P1.substr(0, 3)
                          //  alert(from_y)
                            to_y = P2.substr(0, 3)
                            from_m = P1.substr(3, 2)
                           // alert(from_m)
                            to_m = P2.substr(3, 2)
                          //  alert(to_m)
                            v_ym = "'" + P1 + "'";
                            fy = (to_y - from_y);
                            if (to_y - from_y == 0)
                            {
                                for (i = from_m; i < to_m; i++)
                                {
                               //alert(i)
                                    if (parseInt(from_m)<10)
                                    {
                                        v_ym = v_ym + ',' +"'"+ from_y + '0'+(parseInt(i) + 1)+"'"
                                    }
                                    else
                                    {
                                        v_ym = v_ym + ',' + "'" + from_y + (parseInt(i) + 1) + "'"
                                    }
                                   // v_ym = v_ym + ',' + from_y + (parseInt(from_m)+1)
                                }
                                //alert(parseInt(v_ym))
                            }
                            else if (to_y- from_y >= 1)
                            {
                                //alert(to_y - from_y)
                                for (i = 1; i <= fy+1; i++) {
                                   
                                    if ((to_y - from_y) >= 1 && i==1)
                                    {
                                        //alert("in1")
                                        for (j = from_m; j < 12; j++) {
                                           // alert(to_y - from_y)
                                            if (j < 10) {
                                                v_ym = v_ym + ',' + "'" + from_y + '0' + (parseInt(j) + 1) + "'"
                                            }
                                            else {
                                                v_ym = v_ym + ',' + "'" + from_y + (parseInt(j) + 1) + "'"
                                            }
                                            // v_ym = v_ym + ',' + from_y + (parseInt(from_m)+1)
                                        }

                                    }
                                    else if ((to_y - from_y)  >= 1 && i != 1)
                                    {
                                        //alert("in2")
                                        for (j = 0; j < 12; j++) {
                                            
                                            if (j < 10) {
                                                v_ym = v_ym + ',' + "'" + from_y + '0' + (parseInt(j) + 1) + "'"
                                            }
                                            else {
                                                v_ym = v_ym + ',' + "'" + from_y + (parseInt(j) + 1) + "'"
                                            }
                                            // v_ym = v_ym + ',' + from_y + (parseInt(from_m)+1)
                                        }
                                    }
                                    else if (to_y - from_y == 0) {
                                       // alert("in2")
                                        for (j = 0; j < to_m; j++) {
                                            //alert(i)
                                            if (j< 10) {
                                                v_ym = v_ym + ',' + "'" + to_y + '0' + (parseInt(j) + 1) + "'"
                                            }
                                            else {
                                                v_ym = v_ym + ',' + "'" + to_y + (parseInt(j) + 1) + "'"
                                            }
                                            // v_ym = v_ym + ',' + from_y + (parseInt(from_m)+1)
                                        }
                                        //alert(parseInt(v_ym))
                                    }
                                    from_y = parseInt(from_y) + 1
                                    //alert(from_y)
                                }
                              
                            }
                           
                            T1Load();


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
                }

            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['PR_NO', 'MMCODE', 'MMNAME_C', 'MMNAME_E', 'DATA_YM,', 'IN_PRICE', 'P_INV_QTY', 'IN_QTY', 'OUT_QTY', 'INV_QTY', 'BASE_UNIT', 'MIL_INV_QTY', 'SUM_INV_QTY', 'MATNAME','TOT_BWQTY']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

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
                    //p0: T1QueryForm.getForm().findField('P0').getValue(),
                    P0: P0,
                    P1: v_ym,
                    P2: P2,
                    P3: P3,
                    P4: P4,
                    P5: P5,
                    //p8: T1QueryForm.getForm().findField('P8').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            timeout:1500000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0035/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });
    

    function T1Load() {
        T1Tool.moveFirst();
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
        listeners: {
            beforechange: function (T1Tool, pageData) {
                T1Rec = 0; //disable編修按鈕&刪除按鈕
                T1LastRec = null; //T1Form之資料輸選區清空
            },
            afterrender: function (T1Tool) {
                T1Tool.emptyMsg = '<font color=red>沒有任何資料</font>';
            }
        },
        buttons: [
            {
                itemId: 't1print', text: '列印', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('列印', '是否確定列印？', function (btn, text) {
                        if (btn === 'yes') {
                            showReport();
                        }
                    });
                }
            }, {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'P0', value: P0 }); //SQL篩選條件
                            p.push({ name: 'P1', value: v_ym }); //SQL篩選條件
                            p.push({ name: 'P2', value: P2}); //SQL篩選條件
                            p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                            p.push({ name: 'P4', value: P4 }); //SQL篩選條件
                            p.push({ name: 'P5', value: P1 }); //SQL篩選條件
                         
                            PostForm(T1GetExcel, p);

                        }
                    });
                }
            }
        ]
    });

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: v_ym,
                p2: T1Query.getForm().findField('P2').rawValue,
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('rb1').getGroupValue(),
                p5: T1Query.getForm().findField('P1').rawValue,
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                //html: '<iframe src="' + reportUrl + '?WH_NO=' + WH_NO + '&STORE_LOC=' + STORE_LOC + '&BARCODE_IsUsing=' + BARCODE_IsUsing + '&STATUS=' + STATUS + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',

                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4+ '&p5=' + np.p5 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
                width: 100
            }, {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 160
            }, {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 50
            }, {
                text: "年月",
                dataIndex: 'DATA_YM',
                style: 'text-align:left',
                align: 'left',
                width: 60
            }, {
                text: "進貨單價",
                dataIndex: 'IN_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                text: "民期初量",
                dataIndex: 'P_INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                text: "民進貨量",
                dataIndex: 'IN_QTY',
                align: 'right',
                width: 70
            }, {
                text: "民核撥量",
                dataIndex: 'OUT_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "民庫存量",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "軍庫存量",
                dataIndex: 'MIL_INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "總庫存量",
                dataIndex: 'SUM_INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
           
            }, {
                text: "累計調撥量",
                dataIndex: 'TOT_BWQTY',
                style: 'text-align:left',
                align: 'right',
                width: 100

            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
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

    T1Query.getForm().findField('P1').setValue(new Date(y, m - 1));
    T1Query.getForm().findField('P2').setValue(new Date(y, m - 1 ));
   
});
