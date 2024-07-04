﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1Rec = 0;
    var T1LastRec = null;


    var col1_labelWid = 130;
    var col1_Wid = 280;
    var col2_labelWid = 130;
    var col2_Wid = 260;
    var col3_labelWid = 110;
    var col3_Wid = 300;
    var f2_wid = (col1_Wid + col2_Wid + col3_Wid) / 6;
    var f3_wid = (col1_Wid + col2_Wid + col3_Wid) / 4;
    var f4_wid = (col1_Wid + col2_Wid + col3_Wid) / 5;
    var mLabelWidth = 70;
    var mWidth = 180;
    var Dno;
    var reportUrl = '/Report/A/AA0166.aspx';

    var T1mmCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P5',
        id: 'P5',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelWidth: 55,
        width: 195,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0166/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1QueryForm.getForm().findField('P3').getValue()
            };
        }

    });

    var MatQueryStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM']
    });

    var FlowidQueryStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM']
    });



    function getDefaultValue(isEndDate) {
        var yyyy = new Date().getFullYear() - 1911;
        var m;
        var d = 0;
        if (isEndDate) {
            m = new Date().getMonth() + 1
            d = new Date(yyyy, m, 0).getDate();
        } else {
            m = new Date().getMonth() - 4
            d = new Date(yyyy, m, 0).getDate();

        }
        var mm = m > 10 ? m.toString() : "0" + m.toString();
        var dd = d > 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;

    }

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AA0034/GetMatClassCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    MatQueryStore.removeAll();
                    var tb_vendor = data.etts;
                    console.log(tb_vendor)
                    if (tb_vendor.length > 0) {
                        MatQueryStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < tb_vendor.length; i++) {
                            MatQueryStore.add({ VALUE: tb_vendor[i].VALUE, COMBITEM: tb_vendor[i].COMBITEM });
                        }
                    }
                    MatQueryStore.add({ VALUE: '38', TEXT: '一般物品類', COMBITEM: '一般物品類' });
                    T1QueryForm.getForm().findField('P3').setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();



    var T1QueryForm = Ext.widget({
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
                    xtype: 'datefield',
                    fieldLabel: '報廢日期區間',
                    name: 'P1',
                    id: 'P1',
                    value: getDefaultValue(false),
                    labelWidth: 90,
                    width: 170
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P2',
                    id: 'P2',
                    labelSeparator: '',
                    value: getDefaultValue(true),
                    labelWidth: 20,
                    width: 100
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P3',
                    id: 'P3',
                    store: MatQueryStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    //multiSelect: true,
                    queryMode: 'local',
                    labelWidth: 70,
                    width: 170,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'

                },
                T1mmCode
                , {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 0 0 20',
                    handler: function () {
                        if (T1QueryForm.getForm().isValid()) {
                            T1Load();
                        }
                    }
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
        fields: ['MMCODE', 'DOCNO', 'SEQ', 'FLOWID', 'MAT_CLASS', 'APPID', 'APPTIME', 'APPLY_NOTE', 'BASE_UNIT', 'DISC_CPRICE', 'MMNAME_E', 'APPQTY', 'MMNAME_C', 'POST_TIME','M_AGENNO']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'DOCNO', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p1: T1QueryForm.getForm().findField('P1').rawValue,
                    p2: T1QueryForm.getForm().findField('P2').rawValue,
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    p5: T1QueryForm.getForm().findField('P5').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0166/GetAll',
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
                text: '匯出', border: 1,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '報廢明細表.xls' });
                    p.push({ name: 'p1', value: T1QueryForm.getForm().findField('P1').rawValue });
                    p.push({ name: 'p2', value: T1QueryForm.getForm().findField('P2').rawValue });
                    p.push({ name: 'p3', value: T1QueryForm.getForm().findField('P3').getValue() });
                    p.push({ name: 'p5', value: T1QueryForm.getForm().findField('P5').getValue() });
                    PostForm('/api/AA0166/GetExcel', p);
                }
            }, {
                text: '列印', handler: function () {
                    if (T1Store.getCount() > 0) {
                        showReport();
                    }
                    else {
                        Ext.Msg.alert('訊息', '無資料可列');
                    }
                }
            }
        ]
    });

    function showReport() {
        if (!win) {
            //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
            var np = {
                p1: T1QueryForm.getForm().findField('P1').rawValue,
                p2: T1QueryForm.getForm().findField('P2').rawValue,
                p3: T1QueryForm.getForm().findField('P3').getValue(),
                p5: T1QueryForm.getForm().findField('P5').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p9=' + np.p9 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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


    var T1Grid = Ext.create('Ext.grid.Panel', {
        id: 'T1Grid',
        menuDisabled: true,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1QueryForm]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: '院內碼', dataIndex: 'MMCODE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '物料分類', dataIndex: 'MAT_CLASS', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '中文品名', dataIndex: 'MMNAME_C', align: 'left', style: 'text-align:left', menuDisabled: true, width: 150 },
            { text: '英文品名', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, width: 150 },
            { text: '計量單位', dataIndex: 'BASE_UNIT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 70 },
            { text: '報廢數量', dataIndex: 'APPQTY', align: 'right', style: 'text-align:left', menuDisabled: true, width: 70 },
            { text: '優惠合約單價', dataIndex: 'DISC_CPRICE', align: 'right', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '報廢日期', dataIndex: 'POST_TIME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '申請單號', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '廠商碼', dataIndex: 'M_AGENNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '申請單備註', dataIndex: 'APPLY_NOTE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { header: "", flex: 1 }
        ]
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

    var viewport = Ext.create('Ext.Viewport', {
        id: 'viewport',
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