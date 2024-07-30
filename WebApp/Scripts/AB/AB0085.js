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
    var start_date = "";
    var end_date = "";
    var hosp_code = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    function get_hosp_code() {
        //hosp_code
        Ext.Ajax.request({
            url: '/api/AA0176/GetLoginInfo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    hosp_code = data.msg;
                    debugger
                }
            },
            failure: function (response, options) {

            }
        });
    }

    get_hosp_code();


    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        //name: 'WH_NO',
        name: 'WH_NO',
        id: 'WH_NO',
        fieldLabel: '入庫庫房',
        allowBlank: false,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0085/Whnos',

        //查詢完會回傳的欄位
        extraFields: ['TUSER', 'INID'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
                queryString: T1Query.getForm().findField('WH_NO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                //var f = T2Form.getForm();
                //if (r.get('MMCODE') !== '') {
                //    f.findField("MMCODE").setValue(r.get('MMCODE'));
                //    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                //    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                //    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                //}
            },
            blur: function (field, eOpts) {
                
            }
        }
    });

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '申請日期',
                    name: 'P1',
                    id: 'P1',
                    allowBlank: false,
                    fieldCls: 'required',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    allowBlank: true, // 欄位是否為必填
                    format: 'Xmd',
                    labelWidth: 70,
                    width: 170
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P2',
                    id: 'P2',
                    allowBlank: false,
                    fieldCls: 'required',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    allowBlank: true, // 欄位是否為必填
                    format: 'Xmd',
                    labelWidth: 15,
                    width: 115
                }, wh_NoCombo, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (!T1Query.getForm().findField('P1').getValue() || !T1Query.getForm().findField('P2').getValue()) {
                            Ext.Msg.alert('提醒', '<span style="color:red">日期區間</span>為必填');
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

    Ext.define('ExcelModel', {
        extend: 'Ext.data.Model',
        Property: 'MMCODE',
        fields: [
            { name: 'WH_NAME', cname: '庫房名稱' },
            { name: 'APVQTY', cname: '核發數量' },
            { name: 'MMCODE', cname: '院內碼' },
            { name: 'TOWH', cname: '入庫庫房代碼' },
            { name: 'AGEN_NAMEE', cname: '廠商英文名稱' },
            { name: 'MMNAME_E', cname: '英文品名' }]
    });

    var ExcelStore = Ext.create('Ext.data.Store', {
        model: ExcelModel,
        listeners: {
            beforeload: function (store, options) { },
            load: function (store, records, successful, eOpts) { }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0085/GetExcel',
            reader: {
                type: 'json',
                root: 'etts'
            }
        }
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_E', 'WH_NAME', 'AGEN_NAMEE', 'APVQTY', 'TOWH']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                start_date = T1Query.getForm().findField('P1').rawValue;
                end_date = T1Query.getForm().findField('P2').rawValue;
                var np = {
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    wh_no: T1Query.getForm().findField('WH_NO').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }

        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0085/GetALL',
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
                    if (T1Store.getCount() > 0) {
                        var d1 = start_date;
                        var d2 = end_date;
                        var e = d1 + ',' + d2;
                        var p = new Array();
                        p.push({ name: 'FN', value: '酒精用量統計月報表.xlsx' }); //檔名
                        p.push({ name: 'TS', value: e }); //SQL篩選條件
                        p.push({ name: 'wh_no', value: T1Query.getForm().findField('WH_NO').getValue() }); //SQL篩選條件
                        if (hosp_code == "0") {
                            PostForm('/api/AB0085/GetExcel', p);
                        }
                        else {
                            PostForm('/api/AB0085/GetExcel2', p);

                        }
                    }
                    else
                        Ext.Msg.alert('訊息', '沒有資料');

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
            //dock: 'top',
            //xtype: 'toolbar',
            //layout: 'fit',
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
            width: 100
        }, {
            text: "入庫庫房代碼",
            dataIndex: 'TOWH',
            width: 100

        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 120
        }, {
            text: "核發數量",
            dataIndex: 'APVQTY',
            align: 'right',
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