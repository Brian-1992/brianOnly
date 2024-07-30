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
    //var St_MatclassGet = '../../../api/AA0064/GetMatclassCombo';
    var St_FrwhGet = '../../../api/AA0064/GetFrwhCombo';
    var T1GetExcel = '../../../api/AA0064/Excel';


    var reportUrl = '/Report/A/AA0064.aspx';　　　　　　　　//明細

    // 物料群組
    //var st_matclass = Ext.create('Ext.data.Store', {
    //    fields: ['VALUE', 'COMBITEM']
    //});
    var CLSNAMEStore = Ext.create('Ext.data.Store', {  //物料分類的store
        fields: ['VALUE', 'TEXT']
    });
    var st_frwh = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    function setComboData() {

        //物料分類
        Ext.Ajax.request({
            url: '../../../api/AA0063/GetCLSNAME',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var clsnames = data.etts;
                    tempCLS = clsnames;
                    if (clsnames.length > 0) {
                        CLSNAMEStore.add({ VALUE: '', TEXT: '' });
                        ;
                        for (var i = 0; i < clsnames.length; i++) {
                            CLSNAMEStore.add({ VALUE: clsnames[i].VALUE, TEXT: clsnames[i].TEXT });
                        }
                    }
                    T1Query.getForm().findField("MAT_CLASS").setValue("");
                }
            },
            failure: function (response, options) {

            }
        });

        //繳回庫房
        Ext.Ajax.request({
            url: St_FrwhGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb = data.etts;
                    if (tb.length > 0) {
                        st_frwh.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < tb.length; i++) {
                            st_frwh.add({ VALUE: tb[i].VALUE, COMBITEM: tb[i].COMBITEM });
                        }

                        if (tb.length == 1) {
                            //1筆資料時將
                            T1Query.getForm().findField('FRWH').setValue(tb[0].VALUE);
                        }
                        else {
                            T1Query.getForm().findField('FRWH').setDisabled(false);
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
        queryUrl: '/api/AA0064/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: T1Query.getForm().findField('MAT_CLASS').getValue(),  //MAT_CLASS:預設是動態MMCODE
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
    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    fieldLabel: 'Update',
                    name: 'x',
                    xtype: 'hidden'
                    //},
                    //{
                    //    xtype: 'monthfield',
                    //    fieldLabel: '',
                    //    name: 'D0',
                    //    id: 'D0',
                    //    width: 230
                }, {
                    xtype: 'datefield',
                    fieldLabel: '申請繳回日期區間',
                    labelWidth: 120,
                    width: 205,
                    name: 'APPTIME_START',
                    id: 'APPTIME_START',
                    value: new Date(new Date().getFullYear(), new Date().getMonth() - 6, new Date().getDate()),
                    vtype: 'dateRange',
                    dateRange: { end: 'APPTIME_END' },
                    padding: '0 4 0 4'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: 15,
                    width: 100,
                    name: 'APPTIME_END',
                    id: 'APPTIME_END',
                    labelSeparator: '',
                    //fieldCls: 'required',
                    vtype: 'dateRange',
                    dateRange: { begin: 'APPTIME_START' },
                    value: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate()),
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    fieldLabel: '繳回庫房',
                    name: 'FRWH',
                    id: 'FRWH',
                    store: st_frwh,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: false,
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    width: 300,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>', // 
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'MAT_CLASS',
                    id: 'MAT_CLASS',
                    store: CLSNAMEStore, // st_matclass,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    multiSelect: false,
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
                    }
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load,
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        T1Query.getForm().findField('APPTIME_START').reset();
                        T1Query.getForm().findField('APPTIME_END').reset();
                        T1Query.getForm().findField('MAT_CLASS').reset();
                        T1Query.getForm().findField('FRWH').reset();

                        var f = this.up('form').getForm();
                        f.findField('MAT_CLASS').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                },
                //{
                //    xtype: 'button',
                //    text: '匯出', //T1Query
                //    handler: function () {
                //        var p = new Array();

                //        if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {
                //            p.push({ name: 'FN', value: '明細報表.xls' });
                //        } else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2"){
                //            p.push({ name: 'FN', value: '彙總報表.xls' });
                //        }

                //        if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue != "") {
                //            p.push({ name: 'd0', value: T1Query.getForm().findField('D0').getValue() });
                //            p.push({ name: 'd1', value: T1Query.getForm().findField('D1').getValue() });
                //            p.push({ name: 'MAT_CLASS', value: T1Query.getForm().findField('MAT_CLASS').getValue() });
                //            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                //            p.push({ name: 'p2', value: T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue });
                //            p.push({ name: 'p3', value: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue });

                //            PostForm(T1GetExcel, p);
                //            msglabel('訊息區:匯出完成');
                //        } else {
                //            msglabel('訊息區:需要選匯出選項' + T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue);
                //        }

                //    }
                //}
            ],
        }],

    });

    //var T1Store = Ext.create('WEBAPP.store.ItemAllocation', {
    //    listeners: {
    //        beforeload: function (store, options) {
    //            // 載入前將查詢條件MAT_CLASS的值代入參數
    //            var np = {
    //                //d0: T1Query.getForm().findField('D0').getValue(),
    //                //d1: T1Query.getForm().findField('D1').getValue(),
    //                //MAT_CLASS: T1Query.getForm().findField('MAT_CLASS').getValue(),
    //                //p1: T1Query.getForm().findField('P1').getValue(),
    //                //showopt: T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue,
    //                //showdata: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue,
    //            };
    //            Ext.apply(store.proxy.extraParams, np);
    //        }
    //    }
    //});
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            //{ name: 'DOCNO', type: 'string' },
            //{ name: 'APPDEPT', type: 'string' },
            //{ name: 'ITEM_SUM', type: 'string' },
            //{ name: 'APPQTY_SUM', type: 'string' },
            //{ name: 'LOT_NO', type: 'string' },
            //{ name: 'ACT_PICK_QTY_SUM', type: 'string' },
            //{ name: 'DIFFQTY_SUM', type: 'string' }

            { name: 'DOCNO', type: 'string' }, // 01.單據號碼
            { name: 'MAT_CLASS', type: 'string' }, // 02.物料分類
            { name: 'MAT_CLASS_NAME', type: 'string' }, // 03.物料分類中文
            { name: 'FRWH', type: 'string' }, // 04.庫房
            { name: 'FRWH_NAME', type: 'string' }, // 05.庫房中文名稱
            { name: 'APPTIME', type: 'string' }, // 06.申請日期
            { name: 'APPLY_NOTE', type: 'string' }, // 07.申請單備註
            { name: 'MMCODE', type: 'string' }, // 08.院內碼
            { name: 'MMNAME_C', type: 'string' }, // 09.品名中文
            { name: 'MMNAME_E', type: 'string' }, // 10.品名英文
            { name: 'MMNAME_CE', type: 'string' }, // 11.品名
            { name: 'APPQTY', type: 'string' }, // 12.申請繳回數量
            { name: 'BASE_UNIT', type: 'string' }, // 13.單位
            { name: 'AVG_PRICE', type: 'string' }, // 14.平均單價
            { name: 'DOCTYPE', type: 'string' }, // 15.單據類別
            { name: 'FLOWID', type: 'string' } // 16.流程代碼
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0064/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件MAT_CLASS值代入參數
                var np = {
                    //d0: T1Query.getForm().findField('D0').getValue(),
                    //d1: T1Query.getForm().findField('D1').getValue(),
                    //MAT_CLASS: T1Query.getForm().findField('MAT_CLASS').getValue(),
                    //p1: T1Query.getForm().findField('P1').getValue(),
                    //showopt: T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue,
                    //showdata: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue,
                    APPTIME_START: T1Query.getForm().findField('APPTIME_START').rawValue,
                    APPTIME_END: T1Query.getForm().findField('APPTIME_END').rawValue,
                    MAT_CLASS: T1Query.getForm().findField('MAT_CLASS').getValue(),
                    FRWH: T1Query.getForm().findField('FRWH').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                APPTIME_START: T1Query.getForm().findField('APPTIME_START').getValue(),
                APPTIME_END: T1Query.getForm().findField('APPTIME_END').getValue(),
                MAT_CLASS: T1Query.getForm().findField('MAT_CLASS').getValue(),
                FRWH: T1Query.getForm().findField('FRWH').getValue(),
                start: 0
            }
        });


        T1Tool.moveFirst();
        //var grid = Ext.ComponentQuery.query('grid')[0];
        //if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {      //明細報表
        //    var grid = Ext.ComponentQuery.query('grid')[0];
        //    var col = grid.getView().getHeaderCt().getHeaderAtIndex(8);
        //    var col2 = grid.getView().getHeaderCt().getHeaderAtIndex(10);

        //    col.show();
        //    col2.show();


        //}
        //else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2") { //彙總報表
        //    var grid = Ext.ComponentQuery.query('grid')[0];
        //    var col = grid.getView().getHeaderCt().getHeaderAtIndex(8);
        //    var col2 = grid.getView().getHeaderCt().getHeaderAtIndex(10);


        //    col.hide();
        //    col2.hide();

        //}

        //        msglabel('訊息區:' + T1Query.getForm().findField('MAT_CLASS').getValue());
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
                itemId: 'export', text: '匯出', //T1Query
                handler: function () {
                    var p = new Array();

                    //if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {
                    //    p.push({ name: 'FN', value: '明細報表' });
                    //} else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2") {
                    //    p.push({ name: 'FN', value: '彙總報表' });
                    //}
                    //if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue != "") {
                    p.push({ name: 'APPTIME_START', value: T1Query.getForm().findField('APPTIME_START').rawValue });
                    p.push({ name: 'APPTIME_END', value: T1Query.getForm().findField('APPTIME_END').rawValue });
                    p.push({ name: 'MAT_CLASS', value: T1Query.getForm().findField('MAT_CLASS').getValue() });
                    p.push({ name: 'FRWH', value: T1Query.getForm().findField('FRWH').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                    //} else {
                    //    //msglabel('訊息區:需要選匯出選項' + T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue);
                    //    msglabel('訊息區:');
                    //}

                }
            },
            {
                text: '列印', handler: function () {
                    if (T1Store.getCount() > 0) {
                        //msglabel(T1Query.getForm().findField('D0').getValue() + "," + T1Query.getForm().findField('D1').getValue());

                        //if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {        //明細
                        reportUrl = '/Report/A/AA0064.aspx';
                        showReport();
                        //} else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2") { //彙整
                        //    reportUrl = '/Report/A/AA0064General.aspx';
                        //    showReport();
                        //}
                    }
                    else
                        Ext.Msg.alert('訊息', '請先輸入查詢條件');
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
                html: '<iframe src="' + reportUrl + '?type=' + "AA0064"
                + '&APPTIME_START=' + T1Query.getForm().findField('APPTIME_START').rawValue
                + '&APPTIME_END=' + T1Query.getForm().findField('APPTIME_END').rawValue
                + '&MAT_CLASS=' + T1Query.getForm().findField('MAT_CLASS').getValue()
                + '&FRWH=' + T1Query.getForm().findField('FRWH').getValue()
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
                text: "繳回單號", dataIndex: 'DOCNO', width: 100, sortable: true
            }, {
                text: "物料分類", dataIndex: 'MAT_CLASS_NAME', width: 100, sortable: true
            }, {
                text: "繳回庫房", dataIndex: 'FRWH_NAME', width: 140, sortable: true
            }, {
                text: "申請日期", dataIndex: 'APPTIME', width: 100, sortable: true
            }, {
                text: "院內碼", dataIndex: 'MMCODE', width: 80, sortable: true
            }, {
                text: "中文品名", dataIndex: 'MMNAME_C', width: 200, sortable: true
            }, {
                text: "英文品名", dataIndex: 'MMNAME_E', width: 200, sortable: true
            }, {
                text: "繳回數量", dataIndex: 'APPQTY', width: 80, sortable: true
            }, {
                text: "單位", dataIndex: 'BASE_UNIT', width: 50, sortable: true
            }, {
                text: "平均單價", dataIndex: 'AVG_PRICE', width: 120, sortable: true
            }, {
                text: "申請備註", dataIndex: 'APPLY_NOTE', width: 200, sortable: true
                //    text: "院內碼",
                //    dataIndex: 'MMCODE',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 100
                //}, {
                //    text: "中文品名",
                //    dataIndex: 'MMNAME_C',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 250
                //}, {
                //    text: "英文品名",
                //    dataIndex: 'MMNAME_E',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 250
                //}, {
                //    text: "物料分類名稱",
                //    dataIndex: 'MAT_CLSNAME',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 100
                //}, {
                //    text: "計量單位",
                //    dataIndex: 'BASE_UNIT',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 80
                //}, {
                //    text: "使用單位",
                //    dataIndex: 'INID_NAME_USER',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 150
                //}, {
                //    text: "單位代碼",
                //    dataIndex: 'APPDEPT',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 80
                //}, {
                //    text: "單位名稱",
                //    dataIndex: 'INID_NAME',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 150
                //}, {
                //    text: "核撥年月",
                //    name: '核撥年月',
                //    dataIndex: 'APPTIME_YM',
                //    style: 'text-align:left',
                //    align: 'left',
                //    width: 100
                //}, {
                //    text: "總核撥量",
                //    dataIndex: 'APVQTYN',
                //    style: 'text-align:left',
                //    align: 'right',
                //    width: 100
                //}, {
                //    text: "庫存單價",
                //    dataIndex: 'AVG_PRICE',
                //    style: 'text-align:left',
                //    align: 'right',
                //    width: 100
                //}, {
                //    text: "合約單價",
                //    dataIndex: 'M_CONTPRICE',
                //    style: 'text-align:left',
                //    align: 'right',
                //    width: 100
                //}, {
                //    text: "合約金額",
                //    dataIndex: 'M_ALLPRICE',
                //    style: 'text-align:left',
                //    align: 'right',
                //    width: 100
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

    T1Query.getForm().findField('APPTIME_START').focus();

});