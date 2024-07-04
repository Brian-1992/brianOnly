Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.onReady(function () {
    //var T1Set = ''; // 新增/修改/刪除
    //var T1GetExcel = '../../../api/AA0140/Excel';
    var T1Name = "國軍醫院外傷用戰略物資藥品年報表";

    var T1Rec = 0;
    var T1LastRec = null;

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();
    var reportUrl = '/Report/A/AA0146.aspx';
    var T2GetExcel = '/api/AA0146/Excel';

    //Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_FL_NAME = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function getFL_NAMECombo() {
        Ext.Ajax.request({
            url: '/api/AA0146/GetFL_NAMECombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var fl_name = data.etts;
                    if (fl_name.length > 0) {
                        st_FL_NAME.removeAll();
                        //st_FL_NAME.add({
                        //    VALUE: '',
                        //    TEXT: '全部'
                        //});
                        for (var i = 0; i < fl_name.length; i++) {
                            st_FL_NAME.add({
                                VALUE: fl_name[i].VALUE,
                                TEXT: fl_name[i].TEXT
                            });
                        }
                        //T1Query.getForm().findField('P0').setValue(''); //fl_name
                        //T2Query.getForm().findField('P2').setValue(''); //fl_name 
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getFL_NAMECombo();

    //FL_NAME
    //var st_FL_NAME = Ext.create('Ext.data.Store', {
    //    proxy: {
    //        type: 'ajax',
    //        actionMethods: {
    //            read: 'POST' // by default GET
    //        },
    //        url: '/api/AA0146/GetFL_NAMECombo',
    //        reader: {
    //            type: 'json',
    //            rootProperty: 'etts'
    //        },
    //    },
    //    autoLoad: true
    //});


    //T1Model        //定義有多少欄位參數
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CONTRACNO', type: 'string' },        // 聯標合約項次
            { name: 'MMCODE', type: 'string' },           // 院內碼
            { name: 'MMNAME_E', type: 'string' },         // 商品名
            { name: 'E_SCIENTIFICNAME', type: 'string' }, // 成分名
            { name: 'E_MANUFACT', type: 'string' },       // 廠牌
            { name: 'BASE_UNIT', type: 'string' },        // 單位
            { name: 'M_CONTPRICE', type: 'string' },      // 單價
            { name: 'INV_QTY', type: 'string' },          // 囤儲數量
            { name: 'TOTAL', type: 'string' },            // 金額
            { name: 'AGEN_NAME', type: 'string' },        // 廠商
            { name: 'WResQty', type: 'string' },          // 規定屯量
            { name: 'NOTE', type: 'string' },             // 備考
        ]
    });

    //T1Store         //定義URL取得DB資料
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        //依SEQ排序
        sorters: [{ property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0146/GetAll',  // 呼叫AA0146Control 中的All,再至DB取得資料值 
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: { //觸發
            beforeload: function (store, options) {
                var np = {
                    p0: p0,  //前端取得參數 fl_name
                    p1: p1   //Control_Id
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                //var dataCount = store.getCount();
                //if (dataCount > 0) {  //設定匯出是否disable
                //    Ext.getCmp('btnExcel').setDisabled(false);
                //} else {
                //    Ext.getCmp('btnExcel').setDisabled(true);
                //}
            }
        }
    });

    //T1Query        //查詢條件
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 95
        },
        items: [
            {
                xtype: 'combo',
                store: st_FL_NAME,
                fieldLabel: '管制檔',
                name: 'P0',
                id: 'P0',
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                typeAhead: true,
                forceSelection: true,
                triggerAction: 'all',
                multiSelect: false,
                fieldCls: 'required',
                allowBlank: false,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
            }, {
                xtype: 'radiogroup',
                cls: 'group_check',
                id: 'P1',
                name: 'P1',
                labelAlign: 'right',
                //fieldLabel: '<font color=red>*</font>發生頻率',
                width: 200,
                allowBlank: false,
                //blankText: '您尚未選擇問題發生頻率!',
                items: [
                    { boxLabel: '管制項目', width: 80, name: 'Control_Id', inputValue: 1, checked: true },
                    { boxLabel: '非管制項目', width: 80, name: 'Control_Id', inputValue: 2 },
                ]
            }, {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    var f = T1Query.getForm();
                    if (f.isValid()) {
                        p0 = f.findField('P0').getValue();
                        p1 = f.findField('Control_Id').getGroupValue();

                        T1Load(true);
                        msglabel('');
                    }
                    else {
                        Ext.MessageBox.alert('提示', '請輸入必填欄位');
                    }
                }
            }, {
                xtype: 'button',
                text: '維護查詢檔',
                handler: function () {
                    ReportWindow.setTitle('維護查詢檔');
                    ReportWindow.show();
                }
            }
        ]
    });

    //T1Tool          //工具列
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true, //列示資料筆數
        border: false,
        plain: true,
        buttons: [
            {
                text: '列印',
                id: 'btnPrint',
                name: 'btnPrint',
                handler: function () {
                    showReport();
                }
            }
        ]
    });

    //T1Grid         //表格列
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "聯標合約項次",
            dataIndex: 'CONTRACNO',
            width: 100
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "商品名",
            dataIndex: 'MMNAME_E',
            width: 130
        }, {
            text: "成分名",
            dataIndex: 'E_SCIENTIFICNAME',
            width: 130
        }, {
            text: "廠牌",
            dataIndex: 'E_MANUFACT',
            width: 130
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50
        }, {
            text: "單價",
            dataIndex: 'M_CONTPRICE',
            width: 80
        }, {
            text: "囤儲數量",
            dataIndex: 'INV_QTY',
            width: 80
        }, {
            text: "金額",
            dataIndex: 'TOTAL',
            width: 80
        }, {
            text: "廠商",
            dataIndex: 'AGEN_NAME',
            width: 130
        }, {
            text: "規定屯量",
            dataIndex: 'WRESQTY',
            width: 80
        }, {
            text: "備考",
            dataIndex: 'NOTE',
            width: 130
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0]; //將遊標所在資料寫至T1LastRec
                // setFormT1a();  //T1 Grid資料被點選時的動作
            }
        }
    });

    //T1Load        //將DB資料讀入前端
    function T1Load(moveFirst) {
        if (moveFirst) {
            T1Tool.moveFirst(); //移動到第一頁，與按一下“first”按鈕有相同的效果。
        }
        else {
            T1Store.load({
                params: {
                    start: 0 //start: 0 從第0筆開始顯示,如果要從後端控制每個分頁起始, 可從這邊傳給後端
                }
            });
        }
    }

    //T1Cleanup     //
    //T1Summit     //
    //get_np        //
    //setComboData  //載入下拉選單
    //setFormT1     //設定T1Form
    //setFormT1a    //T1Grid charge 時設定 T1Form

    //T2Model        //定義有多少欄位參數
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'FL_NAME', type: 'string' },      // 管制檔名稱
            { name: 'SEQ_NO', type: 'string' },       // 項次
            { name: 'NOB_NO', type: 'string' },       // 動員序號
            { name: 'MAT_NAME', type: 'string' },     // 類型
            { name: 'MMNAME', type: 'string' },       // 品項
            { name: 'E_SPECNUNIT', type: 'string' },  // 規格
            { name: 'E_DRUGFORM', type: 'string' },   // 劑型類別
            { name: 'WRESQTY', type: 'string' },      // 三總須採購囤儲數量
            { name: 'TRANSQTY', type: 'string' },     // 依包裝規格換算須採購量
            { name: 'PUR_MMCODE', type: 'string' },   // 規劃採購品項院內碼
            { name: 'WRES_MMCODE', type: 'string' },  // 專用戰略物資院內碼
            { name: 'MMNAME_E', type: 'string' },     // 院內品名
            { name: 'MMNAME_C', type: 'string' },     // 中文品名
            { name: 'AGEN_NAME', type: 'string' },    // 合約商
            { name: 'E_ITEMARMYNO', type: 'string' }, // 單價
            { name: 'M_CONTPRICE', type: 'string' },  // 軍聯標項次
            { name: 'DISC_CPRICE', type: 'string' },  // 實售價
            { name: 'PUR_QTY', type: 'string' },      // 採購數量
            { name: 'PUR_AMT', type: 'string' },      // 採購金額
            { name: 'M_STOREID', type: 'string' },    // 採購方式
        ]
    });


    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ_NO', direction: 'ASC' }],//依SEQ_NO排序
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0146/GetControl',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 將FL_NAME代入參數
                var np = {
                    P2: P2  //FL_NAME                    
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T2Load(moveFirst) {
        if (moveFirst) {
            T2Tool.moveFirst(); //移動到第一頁，與按一下“first”按鈕有相同的效果。
        }
        else {
            T2Store.load({
                params: {
                    start: 0 //start: 0 從第0筆開始顯示,如果要從後端控制每個分頁起始, 可從這邊傳給後端
                }
            });
        }
    }

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'combo',
            store: st_FL_NAME,
            fieldLabel: '管制檔',
            name: 'P2',
            id: 'P2',
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            anyMatch: true,
            typeAhead: true,
            forceSelection: true,
            triggerAction: 'all',
            multiSelect: false,
            fieldCls: 'required',
            allowBlank: false,
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
        }, {
            xtype: 'button',
            text: '查詢',
            handler: function () {
                
                var f = T2Query.getForm();
                if (f.isValid()) {
                    P2 = f.findField('P2').getValue();

                    T2Load(true);
                    msglabel('');
                }
                else {
                    Ext.MessageBox.alert('提示', '請輸入必填欄位');
                }
            }
        }, {
            xtype: 'filefield',
            name: 'uploadExcel',
            id: 'uploadExcel',
            buttonText: '匯入',
            buttonOnly: true,
            //padding: '0 4 0 0',
            width: 72,
            listeners: {
                change: function (widget, value, eOpts) {
                    var files = event.target.files; //取得檔案訊息
                    if (!files || files.length == 0) return;//make sure we got something
                    var file = files[0];
                    var ext = this.value.split('.').pop();//取得副檔名
                    if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                        Ext.MessageBox.alert('提示', '僅支援讀取xlsx和xls格式！');
                        Ext.getCmp('import').fileInputEl.dom.value = ''; //??
                        msglabel('');
                    } else {
                        uploadCheckYWindow.show();
                        msglabel("已選擇檔案");
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        var formData = new FormData();
                        formData.append("file", file);
                        var ajaxRequest = $.ajax({
                            type: "POST",
                            url: "/api/AA0146/Upload",
                            data: formData,
                            processData: false,
                            contentType: false, //必須false才會自動加上正確的Content-Type
                            success: function (data, textStatus, jqXHR) {
                                myMask.hide();
                                if (!data.success) {
                                    Ext.MessageBox.alert("提示", data.msg);
                                    msglabel("訊息區:");
                                    Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                                }
                                else {
                                    msglabel("訊息區:資料匯入成功");
                                    Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                                    setT3Store(data.etts);
                                }
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                myMask.hide();
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                                Ext.getCmp('uploadExcel').setRawValue("");
                            }
                        });
                    }
                }
            },
        }, {
            xtype: 'button',
            text: '下載匯入範例',
            handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: '下載匯入範例.xlsx' });
                p.push({ name: 'fl_name', value: T2Query.getForm().findField('P2').getValue() });
                PostForm(T2GetExcel, p);
                msglabel('匯出完成');
            }
        }]
    });

    //T2Tool          //工具列
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true, //列示資料筆數
        border: false,
        plain: true
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        height: windowHeight - 60,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "管制檔名稱",
                dataIndex: 'FL_NAME',
                width: 100
            }, {
                text: "項次",
                dataIndex: 'SEQ_NO',
                width: 40
            }, {
                text: "動員序號",
                dataIndex: 'NOB_NO',
                width: 70
            }, {
                text: "類型",
                dataIndex: 'MAT_NAME',
                width: 60
            }, {
                text: "品項",
                dataIndex: 'MMNAME',
                width: 130
            }, {
                text: "規格",
                dataIndex: 'E_SPECNUNIT',
                width: 130
            }, {
                text: "劑型類別",
                dataIndex: 'E_DRUGFORM',
                width: 90
            }, {
                text: "三總須採購囤儲數量",
                dataIndex: 'WRESQTY',
                width: 130,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000"); }
            }, {
                text: "依包裝規格換算須採購量",
                dataIndex: 'TRANSQTY',
                width: 130,
                style: 'text-align:left',
                align: 'right',
                //renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000"); }
            }, {
                text: "規劃採購品項院內碼",
                dataIndex: 'PUR_MMCODE',
                width: 65
            }, {
                text: "專用戰略物資院內碼",
                dataIndex: 'WRES_MMCODE',
                width: 80
            }, {
                text: "院內品名",
                dataIndex: 'MMNAME_E',
                width: 130
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 130
            }, {
                text: "合約商",
                dataIndex: 'AGEN_NAME',
                width: 130
            }, {
                text: "軍聯標項次",
                dataIndex: 'M_CONTPRICE',
                width: 80
            }, {
                text: "發票單價",
                dataIndex: 'E_ITEMARMYNO',
                width: 70,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "實售價",
                dataIndex: 'DISC_CPRICE',
                width: 70,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) {
                    
                    return Ext.util.Format.number(val, "0,000.00");
                }
            }, {
                text: "採購數量",
                dataIndex: 'PUR_QTY',
                width: 70,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000"); }
            }, {
                text: "採購金額",
                dataIndex: 'PUR_AMT',
                width: 70,
                style: 'text-align:left',
                align: 'right',
                //renderer: function (val, meta, record) {
                    
                //    return Ext.util.Format.number(val, "0,000");
                //}
            }, {
                text: "採購方式",
                dataIndex: 'M_STOREID',
                width: 100
            }, {
                header: "",
                flex: 1
            }
        ]
    });

    //#region uploadCheckYWindow 匯入時檢查資料是否全符合，彈出視窗顯示結果
    function setT3Store(datas) {
        
        T3Store.removeAll();
        var showConfirm = true;
        for (var i = 0; i < datas.length; i++) {
            T3Store.add(datas[i]);
            if (datas[i].SaveStatus == 'N') {
                showConfirm = false;
            }
        }

        if (showConfirm == false) {
            Ext.getCmp('uploadConfirmY').hide();
        } else {
            Ext.getCmp('uploadConfirmY').show();
        }
    }
    var T3Store = Ext.create('Ext.data.Store', {
        fields: ['SEQ_NO', 'NOB_NO', 'MAT_NAME', 'MMNAME', 'E_SPECNUNIT',
            'E_DRUGFORM', 'WRESQTY', 'TRANSQTY', 'PUR_MMCODE', 'WRES_MMCODE',
            'MMNAME_E', 'MMNAME_C', 'AGEN_NAME', 'E_ITEMARMYNO', 'M_CONTPRICE',
            'DISC_CPRICE', 'PUR_QTY', 'PUR_AMT', 'M_STOREID', 'CREATE_TIME',
            'CREATE_USER']
    });

    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T3',
        id: 't3Grid',
        columns: [{
            text: "是否通過",
            dataIndex: 'SaveStatus',
            width: 80,
            renderer: function (val, meta, record) {
                if (record.data.SaveStatus == 'Y') {
                    return '通過';
                }
                if (record.data.SaveStatus == 'N') {
                    return '不通過';
                }
            },
        }, {
            text: "原因",
            dataIndex: 'UploadMsg',
            width: 120,
            renderer: function (value, meta, record) {
                meta.style = 'white-space:normal;word-break:break-all;';
                return value;
            }
        }, {
            text: "項次",
            dataIndex: 'SEQ_NO',
            width: 40
        }, {
            text: "動員序號",
            dataIndex: 'NOB_NO',
            width: 70
        }, {
            text: "類型",
            dataIndex: 'MAT_NAME',
            width: 60
        }, {
            text: "品項",
            dataIndex: 'MMNAME',
            width: 130
        }, {
            text: "規格",
            dataIndex: 'E_SPECNUNIT',
            width: 130
        }, {
            text: "劑型_類別",
            dataIndex: 'E_DRUGFORM',
            width: 90
        }, {
            text: "三總須採購囤儲數量",
            dataIndex: 'WRESQTY',
            width: 130,
            style: 'text-align:left',
            align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000"); }
        }, {
            text: "依包裝規格換算須採購量",
            dataIndex: 'TRANSQTY',
            width: 130,
            style: 'text-align:left',
            align: 'right',
            //renderer: function (val, meta, record) {
            //    
            //    return Ext.util.Format.number(val, "9,999");
            //}
        }, {
            text: "規劃採購品項院內碼",
            dataIndex: 'PUR_MMCODE',
            width: 65
        }, {
            text: "專用戰略物資院內碼",
            dataIndex: 'WRES_MMCODE',
            width: 80,
        }, {
            text: "院內品名",
            dataIndex: 'MMNAME_E',
            width: 130
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 130
        }, {
            text: "合約商",
            dataIndex: 'AGEN_NAME',
            width: 130
        }, {
            text: "軍聯標項次",
            dataIndex: 'M_CONTPRICE',
            width: 80
        }, {
            text: "發票單價",
            dataIndex: 'E_ITEMARMYNO',
            width: 80,
            style: 'text-align:left',
            align: 'right',
            //renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "實售價",
            dataIndex: 'DISC_CPRICE',
            width: 70,
            style: 'text-align:left',
            align: 'right',
            //renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "採購數量",
            dataIndex: 'PUR_QTY',
            width: 70,
            style: 'text-align:left',
            align: 'right',
            //renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000,000,000,000"); }
        }, {
            text: "採購金額",
            dataIndex: 'PUR_AMT',
            width: 70,
            style: 'text-align:left',
            align: 'right',
            //renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000,000,000,000"); }
        }, {
            text: "採購方式",
            dataIndex: 'M_STOREID',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
    });

    var uploadCheckYWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'uploadCheckYWindow',
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        modal: true, //true 掩閉背后的一切
        title: '本次修改項目(有填欄位才會更新)',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't3Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T3Grid]
        }],
        listeners: {
            show: function () {
                uploadCheckYWindow.center();
            }
        },
        buttons: [
            {
                id: 'uploadConfirmY',
                hidden: true,
                text: '確定上傳',
                handler: function () {
                    
                    var temp_datas = T3Store.getData().getRange();
                    var list = [];
                    for (var i = 0; i < temp_datas.length; i++) {
                        list.push(temp_datas[i].data);
                    }

                    Ext.Ajax.request({
                        url: '/api/AA0146/UploadConfirm',
                        method: reqVal_p,
                        params: { data: Ext.util.JSON.encode(list), isYear: 'Y' },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('資料上傳成功');
                                uploadCheckYWindow.hide();
                                getFL_NAMECombo();
                                T2Load(true);                                
                            } else {
                                msglabel('資料上傳失敗');
                                Ext.Msg.alert('提醒', data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            },
            {
                id: 'closeUploadCheckYWindow',
                disabled: false,
                text: '關閉',
                handler: function () {
                    uploadCheckYWindow.hide();
                }
            }
        ]
    });
    //#endregion
    var ReportWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    {
                        xtype: 'panel',
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        border: false,
                        items: [T2Grid]
                    }
                ],
            }
        ],
        width: "1200px",
        height: windowHeight,
        resizable: false,
        draggable: true,
        closable: false,
        y: 0,
        title: "盤點明細管理",
        buttons: [{
            text: '關閉',
            handler: function () {
                ReportWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                ReportWindow.center();
            }
        }
    });
    ReportWindow.hide();

    function showReport() {
        
        var np = {
            p0: T1Query.getForm().findField('P0').getValue(),
            p1: T1Query.getForm().findField('Control_Id').getGroupValue(),
        };
        if (!win) {
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

    //viewport      //前端畫面佈局
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body, //始終渲染在頁面
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
            collapsible: false, //是否可以收縮
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

});
