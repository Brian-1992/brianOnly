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
    var St_MatclassGet = '../../../api/FA0011/GetMatclassCombo';
    var T1GetExcel = '../../../api/FA0011/Excel';


    var reportUrl = '/Report/F/FA0011.aspx';　　　　　　　　//明細

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
                    var tb_matclass = data.etts;
                    T1Query.getForm().findField('SHOWOPT').setValue({ SHOWOPTRB: 1 });   // 初始化RB
                    T1Query.getForm().findField('SHOWDATA').setValue({ SHOWDATARB: 1 }); // 初始化RB


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
        id: 'P1',
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 280,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/FA0011/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                //mat_class: T1Query.getForm().findField('P0').getValue(),  //P0:預設是動態MMCODE
                //store_id: T1Query.getForm().findField('SHOWOPT').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

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
                },
                T1QuryMMCode,
                {
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
                },
                {
                    xtype: 'radiogroup',
                    anchor: '40%',
                    labelWidth: 20,
                    width: 160,
                    name: 'SHOWDATA',
                    items: [
                        { boxLabel: '明細報表', width: 80, name: 'SHOWDATARB', inputValue: 1 },
                        { boxLabel: '彙總報表', width: 80, name: 'SHOWDATARB', inputValue: 2 }
                    ],
                    listeners:
                    {
                        change: function (rg, nVal, oVal, eOpts) {
                            //setVisibleColumns(nVal.SHOWOPT);
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

        return yyyy.toString() + mm ;

    }

    var T1Store = Ext.create('WEBAPP.store.FA0011', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    d0: T1Query.getForm().findField('D0').getValue(),
                    d1: T1Query.getForm().findField('D1').getValue(),
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    showopt: T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue,
                    showdata: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue,
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('export').setDisabled(false);
                    Ext.getCmp('print').setDisabled(false);
                } else {
                    Ext.getCmp('export').setDisabled(true);
                    Ext.getCmp('print').setDisabled(true);
                }
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();

        if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {      //明細報表
            var grid = Ext.ComponentQuery.query('grid')[0];
            var col = grid.getView().getHeaderCt().getHeaderAtIndex(7);
            var col2 = grid.getView().getHeaderCt().getHeaderAtIndex(9);
            var col3 = grid.getView().getHeaderCt().getHeaderAtIndex(10);

            col.show();
            col2.show();
            col3.show();

        }
        else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2") { //彙總報表
            var grid = Ext.ComponentQuery.query('grid')[0];
            var col = grid.getView().getHeaderCt().getHeaderAtIndex(7);
            var col2 = grid.getView().getHeaderCt().getHeaderAtIndex(9);
            var col3 = grid.getView().getHeaderCt().getHeaderAtIndex(10);

            col.hide();
            col2.hide();
            col3.hide();
        }

        //        msglabel('訊息區:' + T1Query.getForm().findField('P0').getValue());
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
                itemId: 'export',
                text: '匯出', //T1Query
                id: 'export',
                name: 'export',
                handler: function () {
                    var p = new Array();

                    if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {
                        p.push({ name: 'FN', value: '明細報表' });
                    } else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2") {
                        p.push({ name: 'FN', value: '彙總報表' });
                    }

                    if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue != "") {
                        p.push({ name: 'd0', value: T1Query.getForm().findField('D0').getValue() });
                        p.push({ name: 'd1', value: T1Query.getForm().findField('D1').getValue() });
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                        p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                        p.push({ name: 'p2', value: T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue });
                        p.push({ name: 'p3', value: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue });

                        PostForm(T1GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    } else {
                        //msglabel('訊息區:需要選匯出選項' + T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue);
                        msglabel('訊息區:');
                    }

                }
            },
            {
                text: '列印',
                id: 'print',
                name: 'print',
                handler: function () {
                    if (T1Store.getCount() > 0) {
                        //msglabel(T1Query.getForm().findField('D0').getValue() + "," + T1Query.getForm().findField('D1').getValue());

                        if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {        //明細
                            reportUrl = '/Report/F/FA0011.aspx';
                            showReport();
                        } else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2") { //彙整
                            reportUrl = '/Report/F/FA0011General.aspx';
                            showReport();
                        }
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
                html: '<iframe src="' + reportUrl + '?type=' + "FA0011"
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
            },  {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "入庫庫房",
                dataIndex: 'TOWH',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
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
                text: "庫存單價",
                dataIndex: 'AVG_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "合約單價(100%)",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "合約總價",
                dataIndex: 'M_ALLPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
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
    Ext.getCmp('export').setDisabled(true);
    Ext.getCmp('print').setDisabled(true);

});