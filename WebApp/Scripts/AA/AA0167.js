Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;

    var St_MatclassGet = '../../../api/AA0167/GetMatclassCombo';
    var T1GetExcel = '../../../api/AA0167/Excel';

    // 物料分類清單
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 申請單狀態清單
    var st_Flowid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        //物料分類
        Ext.Ajax.request({
            url: St_MatclassGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_matclass = data.etts;
                    st_matclass.add({ VALUE: '', TEXT: '' });
                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, TEXT: tb_matclass[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });

        //申請單狀態
        st_Flowid.add({ VALUE: 'RJ1', TEXT: '1 退貨' });
        st_Flowid.add({ VALUE: 'EX1', TEXT: '2 換貨' });
    }

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P4',
        name: 'P4',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 280,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0167/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P2').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 90,
            width: 230
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '申請換貨日期區間',
                    name: 'P0',
                    id: 'P0',
                    vtype: 'dateRange',
                    dateRange: { end: 'P1' },
                    value: getDefaultValue("P0"),
                    labelWidth: 120,
                    width: 250
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: '10px',
                    name: 'P1',
                    id: 'P1',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P0' },
                    value: getDefaultValue("P1"),
                    labelWidth: 25,
                    width: 155
                }, {
                    xtype: 'combo',
                    store: st_matclass,
                    fieldLabel: '物料分類',
                    name: 'P2',
                    id: 'P2',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    anyMatch: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    labelWidth: 70,
                    width: 190
                }, {
                    xtype: 'combo',
                    store: st_Flowid,
                    fieldLabel: '退/換',
                    name: 'P3',
                    id: 'P3',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    anyMatch: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    labelWidth: 50,
                    width: 170
                },
                T1QuryMMCode,
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
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    function getDefaultValue(isEndDate) {
        var yyyy = 0;
        var m = 0;
        if (isEndDate == "P1") {
            yyyy = new Date().getFullYear() - 1911;
            m = new Date().getMonth() + 1;
        } else if (isEndDate == "P0") {    //減6個月
            var date = new Date();
            date.setMonth(date.getMonth() - 5);

            yyyy = date.getFullYear() - 1911;
            m = date.getMonth();
            if (m == 0) {   //因為從目前六月算起，的前六月是12月，但它跑出來是0
                yyyy = yyyy - 1;
                m = 12;
            }
        }

        var d = 0;
        d = new Date().getDate();

        var mm = m >= 10 ? m.toString() : "0" + m.toString();
        var dd = d >= 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;

    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'FLOW_ID', type: 'string' },
            { name: 'MAT_CLSNAME', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'APPLY_NOTE', type: 'string' },
            { name: 'STAT', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' }
        ]

    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'APPTIME', direction: 'ASC' }, { property: 'DOCNO', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0167/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue()

                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
        viewport.down('#form').collapse();
    }

    // toolbar 
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', //T1Query
                handler: function () {
                    var p = new Array();

                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue }); //使用getValue格式會有不同格式
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });

                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');


                }
            },
            {
                text: '列印', handler: function () {
                    if (T1Store.getCount() > 0) {
                        reportUrl = '/Report/A/AA0167.aspx';
                        showReport();
                    }
                    else
                        Ext.Msg.alert('訊息', '沒有資料');
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
                html: '<iframe src="' + reportUrl + '?type=' + "AA0167"
                + '&APPTIME1=' + T1Query.getForm().findField('P0').rawValue
                + '&APPTIME2=' + T1Query.getForm().findField('P1').rawValue
                + '&MAT_CLASS=' + T1Query.getForm().findField('P2').getValue()
                + '&DOCTYPE=' + T1Query.getForm().findField('P3').getValue()
                + '&MMCODE=' + T1Query.getForm().findField('P4').getValue()
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
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 30
        }, {
            text: "單據號碼",
            dataIndex: 'DOCNO',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "申請單狀態",
            dataIndex: 'FLOW_ID',
            style: 'text-align:left',
            align: 'left',
            width: 90
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLSNAME',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "申請部門",
            dataIndex: 'APPDEPT',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "申請日期",
            dataIndex: 'APPTIME',
            style: 'text-align:left',
            align: 'left',
            width: 90
        }, {
            text: "申請單備註",
            dataIndex: 'APPLY_NOTE',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "出/入",
            dataIndex: 'STAT',
            style: 'text-align:left',
            align: 'left',
            width: 60
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            style: 'text-align:left',
            align: 'left',
            width: 260
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 280
        }, {
            text: "數量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            align: 'right',
            width: 60
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            style: 'text-align:left',
            align: 'right',
            width: 60
        }, {
            text: "優惠合約單價",
            dataIndex: 'DISC_CPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 110
        }],
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
        }
        ]
    });

    setComboData();
});