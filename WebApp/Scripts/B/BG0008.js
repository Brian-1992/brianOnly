Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});


var T1GetExcel = '/api/BG0008/Excel';

var MATComboGet = '/api/BG0008/GetMATCombo';

var reportUrl = '/Report/B/BG0008.aspx';


var po_time_b='';
var po_time_e = '';
var mat_class = '' ;
var agen_no = '' ;
var m_storeid = '' ;
var m_contid = '';

// 物品類別清單
var MATQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    function setComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('P0').setValue(wh_nos[0].VALUE);
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
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P0',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    padding: '0 4 4 4',
                    store: MATQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '日期',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 60,
                    width: 145,
                    allowBlank: false,
                    fieldCls: 'required',
                    padding: '0 4 0 4'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 15,
                    width: 100,
                    allowBlank: false,
                    fieldCls: 'required',
                    padding: '0 4 0 4'
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'textfield',
                fieldLabel: '廠商代碼',
                name: 'P3',
                enforceMaxLength: true,
                maxLength: 100,
                width: 170,
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                fieldLabel: '庫備識別',
                name: 'P4',
                enforceMaxLength: true,
                labelWidth: 60,
                width: 140,
                padding: '0 4 0 4',
                store: [
                    { TEXT: '庫備', VALUE: '1'},
                    { TEXT: '非庫備', VALUE: '0' }
                ],
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
            }, {
                xtype: 'combo',
                fieldLabel: '合約種類',
                name: 'P5',
                enforceMaxLength: true,
                labelWidth: 60,
                width: 140,
                padding: '0 4 0 4',
                store: [
                    { TEXT: '合約', VALUE: '0' },
                    { TEXT: '非合約', VALUE: '2' }
                ],
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
            }, {

                xtype: 'button',
                text: '查詢廠商',
                handler: function () {                    
                    var f = T1Query.getForm();
                    if (f.isValid()) {
                        po_time_b = f.findField('P1').getValue();
                        po_time_e = f.findField('P2').getValue();
                        mat_class = f.findField('P0').getValue();
                        agen_no = f.findField('P3').getValue();
                        m_storeid = f.findField('P4').getValue();
                        m_contid = f.findField('P5').getValue();
                        T1Load();
                    }
                    else
                    {
                        Ext.MessageBox.alert('提示', '請輸入必填欄位');
                    }
                }
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('WH_NO').focus(); // 進入畫面時輸入游標預設在P0
                }
            }
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP3',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'displayfield',
                labelSeparator: '',
                fieldLabel: '<span style="color: red">查詢廠商[日期]為訂單日期, 匯出[日期]為申購單日期</span>',
                labelWidth: 300
            }]
        }
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.BG0008', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                var f = T1Query.getForm();
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    po_time_b: f.findField('P1').getValue(),
                    po_time_e: f.findField('P2').getValue(),
                    mat_class: f.findField('P0').getValue(),
                    agen_no: f.findField('P3').getValue(),
                    m_storeid: f.findField('P4').getValue(),
                    m_contid: f.findField('P5').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);

            },
        }
    });



    function T1Load() {
        T1Tool.moveFirst();
    }
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1print', text: '列印廠商', disabled: false,
                handler:
                showReport                   
            
            }, {
                itemId: 'export1', text: '匯出庫備', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            var _po_time_b = po_time_b.getFullYear() + '/' + (po_time_b.getMonth() + 1) + '/' + po_time_b.getDate();
                            var _po_time_e = po_time_e.getFullYear() + '/' + (po_time_e.getMonth() + 1) + '/' + po_time_e.getDate();
                            p.push({ name: 'po_time_b', value: _po_time_b }); //SQL篩選條件
                            p.push({ name: 'po_time_e', value: _po_time_e }); //SQL篩選條件
                            p.push({ name: 'mat_class', value: mat_class }); //SQL篩選條件
                            p.push({ name: 'agen_no', value: agen_no }); //SQL篩選條件
                            p.push({ name: 'm_storeid', value: '1' }); //SQL篩選條件
                            p.push({ name: 'm_contid', value: m_contid }); //SQL篩選條件
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            }, {
                itemId: 'export2', text: '匯出非庫備', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            var _po_time_b  = po_time_b.getFullYear() + '/' + (po_time_b.getMonth() + 1) + '/' + po_time_b.getDate();
                            var _po_time_e = po_time_e.getFullYear() + '/' + (po_time_e.getMonth() + 1) + '/' + po_time_e.getDate();
                            p.push({ name: 'po_time_b', value: _po_time_b }); //SQL篩選條件
                            p.push({ name: 'po_time_e', value: _po_time_e }); //SQL篩選條件
                            p.push({ name: 'mat_class', value: mat_class }); //SQL篩選條件
                            p.push({ name: 'agen_no', value: agen_no }); //SQL篩選條件
                            p.push({ name: 'm_storeid', value: '0' }); //SQL篩選條件
                            p.push({ name: 'm_contid', value: m_contid }); //SQL篩選條件
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            },
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
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 60
            }, {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 120
            }, {
                text: "訂單編號",
                dataIndex: 'PO_NO',
                width: 130
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 280
            }, {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 280
            }, {
                text: "計量單位",
                dataIndex: 'M_PURUN',
                width: 80
            }, {
                text: "訂單數量",
                dataIndex: 'PO_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                text: "單價",
                dataIndex: 'PO_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "進貨量",
                dataIndex: 'DELI_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "未進貨量",
                dataIndex: 'NOIN_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#t1print').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#export1').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#export2').setDisabled(T1Store.getCount() === 0);
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

    function showReport() {
        if (!win) {
            var _po_time_b = po_time_b.getFullYear() + '/' + (po_time_b.getMonth() + 1) + '/' + po_time_b.getDate();
            var _po_time_e = po_time_e.getFullYear() + '/' + (po_time_e.getMonth() + 1) + '/' + po_time_e.getDate();
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                + '?po_time_b=' + _po_time_b
                + '&po_time_e=' + _po_time_e
                + '&mat_class=' + mat_class
                + '&agen_no=' + agen_no
                + '&m_storeid=' + m_storeid
                + '&m_contid=' + m_contid
                + '&Mat_classRawValue=' + T1Query.getForm().findField('P0').rawValue.substring(3)
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }

    T1Query.getForm().findField('P1').setValue(new Date());
    T1Query.getForm().findField('P2').setValue(new Date());

});
