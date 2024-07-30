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
    var St_MatclassGet = '../../../api/FA0073/GetMatclassCombo';
    var St_TowhGet = '../../../api/FA0073/GetTowhCombo';

    var T1GetExcel = '../../../api/FA0073/Excel';


    var reportUrl = '/Report/F/FA0073.aspx';　　　　　　　　//明細

    // 物料群組
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 入庫庫房
    var st_towh = Ext.create('Ext.data.Store', {
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
                    var tb_matclass = data.etts;
                    T1Query.getForm().findField('SHOWOPT').setValue({ SHOWOPTRB: 1 });   // 初始化RB


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

        //入庫庫房
        Ext.Ajax.request({
            url: St_TowhGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_towh = data.etts;


                    if (tb_towh.length > 0) {
                        T1Query.getForm().findField('P1').setDisabled(false);
                        for (var i = 0; i < tb_towh.length; i++) {
                            st_towh.add({ VALUE: tb_towh[i].VALUE, COMBITEM: tb_towh[i].COMBITEM });
                        }
                    }
                    else
                        T1Query.getForm().findField('P1').setDisabled(true);

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
                    fieldLabel: 'Update',
                    name: 'x',
                    xtype: 'hidden'
                },
                {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P0',
                    id: 'P0',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: true,
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '核撥年月',
                    name: 'D0',
                    id: 'D0',
                    value: getDefaultValue(),
                    width: 160
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'D1',
                    id: 'D1',
                    labelWidth: 7,
                    value: getDefaultValue(),
                    width: 120
                },{
                    xtype: 'combo',
                    fieldLabel: '入庫庫房',
                    store: st_towh,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    anyMatch: true,
                    queryMode: 'local',
                    name: 'P1',
                    id: 'P1',
                    width: 250,
                    listConfig:
                    {
                        width: 250
                    },
                    matchFieldWidth: false,
                    disabled: true,
                    listeners: {
                        select: function (ele, newValue, oldValue) {
                            st_docno.load();
                        }
                    }
                },{
                    xtype: 'radiogroup',
                    anchor: '40%',
                    labelWidth: 25,
                    width: 110,
                    name: 'SHOWOPT',
                    items: [
                        { boxLabel: '庫備', width: 50, name: 'SHOWOPTRB', inputValue: 1 },
                        { boxLabel: '非庫備', width: 60, name: 'SHOWOPTRB', inputValue: 0 }
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
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load,
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        T1Query.getForm().findField('D0').reset();
                        T1Query.getForm().findField('D1').reset();
                        T1Query.getForm().findField('P0').reset();
                        T1Query.getForm().findField('P1').reset();

                        var f = this.up('form').getForm();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                },
            ],
        }],

    });

    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;

        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;


        var mm = m >= 10 ? m.toString() : "0" + m.toString();

        return yyyy.toString() + mm;

    }

    var T1Store = Ext.create('WEBAPP.store.FA0073', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    d0: T1Query.getForm().findField('D0').getValue(),
                    d1: T1Query.getForm().findField('D1').getValue(),
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    showopt: T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue,
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('print').setDisabled(false);
                } else {
                    Ext.getCmp('print').setDisabled(true);
                }
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
                text: '列印',
                id: 'print',
                name: 'print',
                handler: function () {
                    if (T1Store.getCount() > 0) {
                        showReport();
                    }
                    else
                        Ext.Msg.alert('訊息', '請先建立明細資料');
                }
            },

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
                html: '<iframe src="' + reportUrl + '?type=' + "FA0073"
                + '&APPTIME1=' + T1Query.getForm().findField('D0').getValue()
                + '&APPTIME2=' + T1Query.getForm().findField('D1').getValue()
                + '&task_id=' + T1Query.getForm().findField('P0').getValue()
                + '&appdept=' + T1Query.getForm().findField('P1').getValue()
                + '&showopt=' + T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue
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
                text: "入庫庫房代碼",
                dataIndex: 'TOWH',
                style: 'text-align:left',
                align: 'left',
                width: 110
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 150
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 80
            },  {
                text: "核撥年月",
                dataIndex: 'APPTIME_YM',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "核撥總量",
                dataIndex: 'APVQTYN',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "核撥單價",
                //dataIndex: 'AVG_PRICE',
                dataIndex: 'DISC_CPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "核撥成本",
                dataIndex: 'M_ALLPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "成本中心代碼",
                dataIndex: 'INID',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "成本中心名稱",
                dataIndex: 'INID_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 150
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
        },
        ]
    });

    T1Query.getForm().findField('P0').focus();
    Ext.getCmp('print').setDisabled(true);

});