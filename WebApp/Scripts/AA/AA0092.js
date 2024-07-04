Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

//搜尋院內碼
var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
    name: 'MMCODE',
    fieldLabel: '院內碼',


    //width: 150,

    //限制一次最多顯示10筆
    limit: 10,

    //指定查詢的Controller路徑
    queryUrl: '/api/AA0092/GetMMCodeCombo',

    //查詢完會回傳的欄位
    extraFields: ['MAT_CLASS', 'BASE_UNIT'],
    listeners: {
        select: function (c, r, i, e) {
            //選取下拉項目時，顯示回傳值

        }
    }
});

var T1GetExcel = '/api/AA0092/Excel';

var MMCODE = '';
var WH_NO = '';
var p2 = '';


Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    var st_whno = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AA0092/GetWhnoCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    st_whno.add({ VALUE: '', TEXT: '全部', COMBITEM: '全部' });
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            st_whno.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

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
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combobox',
                    fieldLabel: '庫別代碼',
                    name: 'WH_NO',
                    id: 'WH_NO',
                    queryMode: 'local',
                    width: 150,
                    store: st_whno,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    value: '',
                    padding: '0 4 0 4',
                    matchFieldWidth: false,
                    listConfig: { width: 200 }
                },
                mmCodeCombo
                , {
                    xtype: 'combobox',
                    fieldLabel: '是否疫苗',
                    name: 'P2',
                    id: 'P2',
                    queryMode: 'local',
                    displayField: 'name',
                    valueField: 'abbr',
                    width: 145,
                    store: [
                        { abbr: '', name: '全部' },
                        { abbr: 'Y', name: '是' },
                        { abbr: 'N', name: '否' }
                    ],
                    value:'',
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();                        
                        MMCODE = f.findField('MMCODE').getValue();
                        WH_NO = f.findField('WH_NO').getValue();
                        p2 = f.findField('P2').getValue();
                        T1Load();
                    }                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('WH_NO').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }


            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0092', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    MMCODE: MMCODE,
                    WH_NO: WH_NO,
                    p2: p2
                };
                Ext.apply(store.proxy.extraParams, np);

            },             
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
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'MMCODE', value: T1Query.getForm().findField('MMCODE').getValue() }); //SQL篩選條件
                            p.push({ name: 'WH_NO', value: T1Query.getForm().findField('WH_NO').getValue() }); //SQL篩選條件
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                            PostForm(T1GetExcel, p);
                        }
                    });
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
            layout:'fit',
            items: [T1Query]
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
                text: "庫別",
                dataIndex: 'WH_NO',
                width: 85
            }, {
                text: "藥品藥材代碼",
                dataIndex: 'MMCODE',
                width: 110
            }, {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 280
            }, {
                text: "現有存量",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "是否疫苗",
                dataIndex: 'VACKIND',
                width: 80
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
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

    //var d = new Date();
    //m = d.getMonth(); //current month
    //y = d.getFullYear(); //current year

    //T1Query.getForm().findField('P0').focus();
    //T1Query.getForm().findField('P3').setValue(new Date(y, m, 1));
    //T1Query.getForm().findField('P4').setValue(d);

});
